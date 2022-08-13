using System;
using System.Collections.Generic;
using System.Threading;
using ColossalFramework;
using ColossalFramework.Math;
using ICities;

namespace CampusIndustriesHousingMod 
{
    public class StudentManager : ThreadingExtensionBase {
        private const bool LOG_STUDENTS = true;

        private const int DEFAULT_NUM_SEARCH_ATTEMPTS = 3;

        private static StudentManager instance;

        private readonly BuildingManager buildingManager;
        private readonly CitizenManager citizenManager;

        private readonly uint[] familiesWithStudents;
        private readonly uint[] studentsMovingOut;
        private readonly HashSet<uint> studentsBeingProcessed;
        private uint numFamiliesWithStudents;
        private uint numStudentsMoveOut;

        private Randomizer randomizer;

        private int refreshTimer;
        private int running;

        public StudentManager() {
            Logger.logInfo(LOG_STUDENTS, "StudentManager Created");
            instance = this;

            this.randomizer = new Randomizer((uint) 73);
            this.citizenManager = Singleton<CitizenManager>.instance;
            this.buildingManager = Singleton<BuildingManager>.instance;

            uint numCitizenUnits = this.citizenManager.m_units.m_size;

            this.familiesWithStudents = new uint[numCitizenUnits];

            this.studentsMovingOut = new uint[numCitizenUnits];

            this.studentsBeingProcessed = new HashSet<uint>();

            this.numFamiliesWithStudents = 0;

            this.numStudentsMoveOut = 0;
        }

        public static StudentManager getInstance() {
            return instance;
        }

        public override void OnBeforeSimulationTick() {
            // Refresh every every so often
            if (this.refreshTimer++ % 600 == 0) {
                // Make sure refresh can occur, otherwise set the timer so it will trigger again next try
                if (Interlocked.CompareExchange(ref this.running, 1, 0) == 1) {
                    this.refreshTimer = 0;
                    return;
                }

                // Refresh the Students Array
                this.refreshStudents();

                // Reset the timer and running flag
                this.refreshTimer = 1;
                this.running = 0;
            }
        }

        private void refreshStudents() {
            CitizenUnit[] citizenUnits = this.citizenManager.m_units.m_buffer;
            this.numFamiliesWithStudents = 0;
            this.numStudentsMoveOut = 0;
            for (uint i = 0; i < citizenUnits.Length; i++) {
                for (int j = 0; j < 5; j++) {
                    uint citizenId = citizenUnits[i].GetCitizen(j);
                    if (this.validateStudent(citizenId)) {
                        if(this.isMovingIn(citizenId))
                        {
                            this.familiesWithStudents[this.numFamiliesWithStudents++] = i;
                            break;
                        }
                        else if(this.isMovingOut(citizenId))
                        {
                            this.studentsMovingOut[this.numStudentsMoveOut++] = i;
                            break;
                        }
                    }
                }
            }
        }

        public uint[] getFamilyWithStudents(Building buildingData) {
            return this.getFamilyWithStudents(DEFAULT_NUM_SEARCH_ATTEMPTS, buildingData);
        }

        public uint[] getDormApartmentStudents(Building buildingData) {
            return this.getDormApartmentStudents(DEFAULT_NUM_SEARCH_ATTEMPTS, buildingData);
        }

        public uint[] getFamilyWithStudents(int numAttempts, Building buildingData) {
            Logger.logInfo(LOG_STUDENTS, "StudentManager.getFamilyWithStudents -- Start");
            // Lock to prevent refreshing while running, otherwise bail
            if (Interlocked.CompareExchange(ref this.running, 1, 0) == 1) {
                return null;
            }

            // Get random family that contains at least one campus area student
            uint[] family = this.getFamilyWithStudentsInternal(numAttempts, buildingData);
            if (family == null) {
                Logger.logInfo(LOG_STUDENTS, "StudentManager.getFamilyWithStudent -- No Family");
                this.running = 0;
                return null;
            }

            // Mark student as being processed
            foreach (uint familyMember in family) {
                if(this.isCampusStudent(familyMember, buildingData))
                {
                    this.studentsBeingProcessed.Add(familyMember);
                }
            }

            Logger.logInfo(LOG_STUDENTS, "StudentManager.getFamilyWithStudent -- Finished: {0}", string.Join(", ", Array.ConvertAll(family, item => item.ToString())));
            this.running = 0;
            return family;
        }

        public uint[] getDormApartmentStudents(int numAttempts, Building buildingData) {
            Logger.logInfo(LOG_STUDENTS, "StudentManager.getDormApartmentStudents -- Start");
            // Lock to prevent refreshing while running, otherwise bail
            if (Interlocked.CompareExchange(ref this.running, 1, 0) == 1) {
                return null;
            }

            // Get random apartment from the dorms
            uint[] dorms_apartment = this.getDormApartmentStudentsInternal(numAttempts, buildingData);
            if (dorms_apartment == null) {
                Logger.logInfo(LOG_STUDENTS, "StudentManager.getDormApartmentStudentInternal -- No students in this apartment");
                this.running = 0;
                return null;
            }

            // Mark student as being processed
            foreach (uint student in dorms_apartment) {
                if(this.isCampusStudent(student, buildingData))
                {
                    this.studentsBeingProcessed.Add(student);
                }
            }

            Logger.logInfo(LOG_STUDENTS, "StudentManager.getDormApartmentStudentInternal -- Finished: {0}", string.Join(", ", Array.ConvertAll(dorms_apartment, item => item.ToString())));
            this.running = 0;
            return dorms_apartment;
        }

        public void doneProcessingStudent(uint studentId) 
        {
          this.studentsBeingProcessed.Remove(studentId);
        }

        private uint[] getFamilyWithStudentsInternal(int numAttempts, Building buildingData) {
            // Check to see if too many attempts already
            if (numAttempts <= 0) 
            {
                return null;
            }

            // Get a random family with students
            uint familyId = this.fetchRandomFamilyWithStudents();
                
            Logger.logInfo(LOG_STUDENTS, "StudentManager.getFamilyWithStudentsInternal -- Family Id: {0}", familyId);
            if (familyId == 0) 
            {
                // No Family with students to be located
                return null;
            }

            // Validate all students in the family and build an array of family members
            CitizenUnit familyWithStudents = this.citizenManager.m_units.m_buffer[familyId];
            uint[] family = new uint[5];
            bool studentPresent = false;
            for (int i = 0; i < 5; i++) {
                uint familyMember = familyWithStudents.GetCitizen(i);
                if (this.isCampusStudent(familyMember, buildingData)) {
                    if (!this.validateStudent(familyMember)) {
                        // This particular Student is no longer valid for some reason, call recursively with one less attempt
                        return this.getFamilyWithStudentsInternal(--numAttempts, buildingData);
                    }
                    studentPresent = true;
                }
                Logger.logInfo(LOG_STUDENTS, "StudentManager.getFamilyWithStudentsInternal -- Family Member: {0}", familyMember);
                family[i] = familyMember;
            }

            if (!studentPresent) {
                // No Student was found in this family (which is a bit weird), try again
                return this.getFamilyWithStudentsInternal(--numAttempts, buildingData);
            }

            return family;
           
        }

        private uint[] getDormApartmentStudentsInternal(int numAttempts, Building buildingData) {
            // Check to see if too many attempts already
            if (numAttempts <= 0) 
            {
                return null;
            }

            // Get a random dorm apartment with students
            uint dormApartmentId = this.fetchRandomDormApartment();  

            Logger.logInfo(LOG_STUDENTS, "StudentManager.getDormApartmentStudentsInternal -- Family Id: {0}", dormApartmentId);
            if (dormApartmentId == 0) 
            {
                // No dorm apartments to be located
                return null;
            }

            // create an array of students to move out of the apartment
            CitizenUnit dormApartment = this.citizenManager.m_units.m_buffer[dormApartmentId];
            uint[] dorm_apartment = new uint[] {0, 0, 0, 0, 0};
            for (int i = 0; i < 5; i++) 
            {
                uint studentId = dormApartment.GetCitizen(i);
                Logger.logInfo(LOG_STUDENTS, "StudentManager.getDormApartmentStudentsInternal -- Family Member: {0}", studentId);
                // not a campus area student or this campus area student -> move out
                if(studentId != 0 && !this.isCampusStudent(studentId, buildingData))
                {
                    Logger.logInfo(LOG_STUDENTS, "StudentManager.getDormApartmentStudentsInternal -- Family Member: {0}, is not a student", studentId);
                    dorm_apartment[i] = studentId;
                } 
            }

            return dorm_apartment;
 
        }

        private uint fetchRandomFamilyWithStudents() 
        {
            if (this.numFamiliesWithStudents <= 0) 
            {
                return 0;
            }

            int index = this.randomizer.Int32(this.numFamiliesWithStudents);
            return this.familiesWithStudents[index];
        }

        private uint fetchRandomDormApartment() 
        {
            if (this.numStudentsMoveOut <= 0) 
            {
                return 0;
            }

            int index = this.randomizer.Int32(this.numStudentsMoveOut);
            return this.studentsMovingOut[index];
        }

        public bool isCampusStudent(uint studentId, Building buildingData) {
            if (studentId == 0) {
                return false;
            }

            // Validate not dead
            if (this.citizenManager.m_citizens.m_buffer[studentId].Dead) {
                return false;
            }

            // validate is student
            if ((this.citizenManager.m_citizens.m_buffer[studentId].m_flags & Citizen.Flags.Student) == 0) {
                return false;
            }

            // Validate learning in a campus and the same campus as the dorms building
            if(!this.checkCampusArea(studentId, buildingData)) {
                return false;
            }

            return true;
        }

        private bool checkCampusArea(uint studentId, Building buildingData)
        {
            ushort studyBuildingId = this.citizenManager.m_citizens.m_buffer[studentId].m_workBuilding;
            Building studyBuilding = buildingManager.m_buildings.m_buffer[studyBuildingId];

            DistrictManager districtManager = Singleton<DistrictManager>.instance;

            var dorms_park = districtManager.GetPark(buildingData.m_position); // dorms campus park according to dorms position
            var study_park = districtManager.GetPark(studyBuilding.m_position); // student campus park according to workplace position

            // same industry park
            if(buildingData.Info.m_buildingAI is DormsAI dorms && dorms_park == study_park)
            {
                // same industrial area 
                if(studyBuilding.Info.m_buildingAI is CampusBuildingAI campusBuilding && dorms.m_campusType == campusBuilding.m_campusType)
                {
                    return true;
                }
                if(studyBuilding.Info.m_buildingAI is MainCampusBuildingAI mainCampusBuilding && dorms.m_campusType == mainCampusBuilding.m_campusType)
                {
                    return true;
                }
            }

            return false;
        }

        private bool validateStudent(uint studentId) 
        {
            // Validate this Student is not already being processed
            if (this.studentsBeingProcessed.Contains(studentId)) 
            {
                return false;
            }

            return true;
        }

        private bool isMovingIn(uint citizenId)
        {
            ushort homeBuildingId = this.citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
            ushort studyBuildingId = this.citizenManager.m_citizens.m_buffer[citizenId].m_workBuilding;

            // no home or no study
            if (homeBuildingId == 0 || studyBuildingId == 0) 
            {
                return false;
            }

            Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];
            Building studyBuilding = buildingManager.m_buildings.m_buffer[studyBuildingId];

            // not a student
            if ((this.citizenManager.m_citizens.m_buffer[citizenId].m_flags & Citizen.Flags.Student) == 0) {
                return false;
            }

            // not studying in a campus
            if(studyBuilding.Info.m_buildingAI is not CampusBuildingAI && studyBuilding.Info.m_buildingAI is not MainCampusBuildingAI)
            {
                return false;
            }

            // if already living in a dorm
            if(homeBuilding.Info.m_buildingAI is DormsAI)
            {
                return false;
            } 

            return true;
        }

        private bool isMovingOut(uint citizenId)
        {
            // if this student is living in the dorms we should check the entire apartment
            ushort homeBuildingId = this.citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
            Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];
            if(homeBuilding.Info.m_buildingAI is DormsAI)
            {
                return true;
            }

            return false;
        }
    }
}