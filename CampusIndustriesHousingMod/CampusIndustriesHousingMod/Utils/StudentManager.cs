using System;
using System.Collections.Generic;
using System.Threading;
using CampusIndustriesHousingMod.AI;
using ColossalFramework;
using ColossalFramework.Math;
using ICities;

namespace CampusIndustriesHousingMod.Utils  
{
    public class StudentManager : ThreadingExtensionBase 
    {
        private const int DEFAULT_NUM_SEARCH_ATTEMPTS = 3;

        private static StudentManager instance;

        private readonly BuildingManager buildingManager;
        private readonly CitizenManager citizenManager;

        private readonly uint[] familiesWithStudents;
        private readonly uint[] studentsMovingOutUniversity;
        private readonly uint[] studentsMovingOutLiberalArts;
        private readonly uint[] studentsMovingOutTradeSchool;

        private readonly HashSet<uint> studentsBeingProcessed;
        private uint numFamiliesWithStudents;

        private uint numStudentsMoveOutUniversity;
        private uint numStudentsMoveOutLiberalArts;
        private uint numStudentsMoveOutTradeSchool;

        private Randomizer randomizer;

        private int running;

        private const int StepMask = 0xFF;
        private const int BuildingStepSize = 192;
        private ushort studentCheckStep;

        private int studentCheckCounter;

        public StudentManager() 
        {
            Logger.LogInfo(Logger.LOG_STUDENTS, "StudentManager Created");
            instance = this;

            this.randomizer = new Randomizer((uint) 73);
            this.citizenManager = Singleton<CitizenManager>.instance;
            this.buildingManager = Singleton<BuildingManager>.instance;

            uint numCitizenUnits = this.citizenManager.m_units.m_size;

            this.familiesWithStudents = new uint[numCitizenUnits];

            this.studentsMovingOutUniversity = new uint[numCitizenUnits];

            this.studentsMovingOutLiberalArts = new uint[numCitizenUnits];

            this.studentsMovingOutTradeSchool = new uint[numCitizenUnits];

            this.studentsBeingProcessed = [];

            this.numFamiliesWithStudents = 0;

            this.numStudentsMoveOutUniversity = 0;

            this.numStudentsMoveOutLiberalArts = 0;

            this.numStudentsMoveOutTradeSchool = 0;
        }

        public static StudentManager getInstance() 
        {
            return instance;
        }

        public override void OnBeforeSimulationFrame()
        {
            uint currentFrame = SimulationManager.instance.m_currentFrameIndex;
            ProcessFrame(currentFrame);
        }

        public void ProcessFrame(uint frameIndex)
        {
            RefreshStudents();

            if ((frameIndex & StepMask) != 0)
            {
                return;
            }
        }

        private void RefreshStudents()
        {
            if (studentCheckCounter > 0)
            {
                --studentCheckCounter;
                return;
            }

            if (Interlocked.CompareExchange(ref this.running, 1, 0) == 1)
            {
                return;
            }

            ushort step = studentCheckStep;
            studentCheckStep = (ushort)((step + 1) & StepMask);

            RefreshStudents(step);

            this.running = 0;
        }

        private void RefreshStudents(uint step) 
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            this.numFamiliesWithStudents = 0;
            this.numStudentsMoveOutUniversity = 0;
            this.numStudentsMoveOutLiberalArts = 0;
            this.numStudentsMoveOutTradeSchool = 0;

            ushort first = (ushort)(step * BuildingStepSize);
            ushort last = (ushort)((step + 1) * BuildingStepSize - 1);

            for (ushort i = first; i <= last; ++i)
            {
                var building = buildingManager.m_buildings.m_buffer[i];
                if (building.Info.GetAI() is not ResidentialBuildingAI && building.Info.GetAI() is not DormsAI)
                {
                    continue;
                }
                if ((building.m_flags & Building.Flags.Created) == 0)
                {
                    continue;
                }

                uint num = building.m_citizenUnits;
                int num2 = 0;
                while (num != 0)
                {
                    var citizenUnit = instance.m_units.m_buffer[num];
                    uint nextUnit = citizenUnit.m_nextUnit;
                    if ((instance.m_units.m_buffer[num].m_flags & CitizenUnit.Flags.Home) != 0 && !citizenUnit.Empty())
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            uint citizenId = citizenUnit.GetCitizen(j);
                            Citizen citizen = citizenManager.m_citizens.m_buffer[citizenId];
                            if (citizen.m_flags.IsFlagSet(Citizen.Flags.Created) && this.validateStudent(citizenId))
                            {
                                if (this.isMovingIn(citizenId))
                                {
                                    this.familiesWithStudents[this.numFamiliesWithStudents++] = num;
                                    break;
                                }
                                else if (this.isMovingOutUniversity(citizenId))
                                {
                                    this.studentsMovingOutUniversity[this.numStudentsMoveOutUniversity++] = num;
                                    break;
                                }
                                else if (this.isMovingOutLiberalArts(citizenId))
                                {
                                    this.studentsMovingOutLiberalArts[this.numStudentsMoveOutLiberalArts++] = num;
                                    break;
                                }
                                else if (this.isMovingOutTradeSchool(citizenId))
                                {
                                    this.studentsMovingOutTradeSchool[this.numStudentsMoveOutTradeSchool++] = num;
                                    break;
                                }
                            }
                        }
                    }
                    num = nextUnit;
                    if (++num2 > 524288)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                }
            }
        }

        public uint[] getFamilyWithStudents(Building buildingData) 
        {
            return this.getFamilyWithStudents(DEFAULT_NUM_SEARCH_ATTEMPTS, buildingData);
        }

        public uint[] getDormApartmentStudents(Building buildingData) 
        {
            return this.getDormApartmentStudents(DEFAULT_NUM_SEARCH_ATTEMPTS, buildingData);
        }

        public uint[] getFamilyWithStudents(int numAttempts, Building buildingData) 
        {
            Logger.LogInfo(Logger.LOG_STUDENTS, "StudentManager.getFamilyWithStudents -- Start");
            // Lock to prevent refreshing while running, otherwise bail
            if (Interlocked.CompareExchange(ref this.running, 1, 0) == 1) 
            {
                return null;
            }

            // Get random family that contains at least one campus area student
            uint[] family = this.getFamilyWithStudentsInternal(numAttempts, buildingData);
            if (family == null) 
            {
                Logger.LogInfo(Logger.LOG_STUDENTS, "StudentManager.getFamilyWithStudentsInternal -- No Family");
                this.running = 0;
                return null;
            }

            // Mark student as being processed
            foreach (uint familyMember in family) 
            {
                if(this.isCampusAreaStudent(familyMember))
                {
                    this.studentsBeingProcessed.Add(familyMember);
                }
            }

            Logger.LogInfo(Logger.LOG_STUDENTS, "StudentManager.getFamilyWithStudents -- Finished: {0}", string.Join(", ", Array.ConvertAll(family, item => item.ToString())));
            this.running = 0;
            return family;
        }

        public uint[] getDormApartmentStudents(int numAttempts, Building buildingData) 
        {
            Logger.LogInfo(Logger.LOG_STUDENTS, "StudentManager.getDormApartmentStudents -- Start");
            // Lock to prevent refreshing while running, otherwise bail
            if (Interlocked.CompareExchange(ref this.running, 1, 0) == 1) 
            {
                return null;
            }

            // Get random apartment from the dorms
            uint[] dorms_apartment = this.getDormApartmentStudentsInternal(numAttempts, buildingData);
            if (dorms_apartment == null) 
            {
                Logger.LogInfo(Logger.LOG_STUDENTS, "StudentManager.getDormApartmentStudentsInternal -- No students in this apartment");
                this.running = 0;
                return null;
            }

            // Mark student as being processed
            foreach (uint student in dorms_apartment) 
            {
                if(this.isCampusAreaStudent(student))
                {
                    this.studentsBeingProcessed.Add(student);
                }
            }

            Logger.LogInfo(Logger.LOG_STUDENTS, "StudentManager.getDormApartmentStudentInternal -- Finished: {0}", string.Join(", ", Array.ConvertAll(dorms_apartment, item => item.ToString())));
            this.running = 0;
            return dorms_apartment;
        }

        public void doneProcessingStudent(uint studentId) 
        {
          this.studentsBeingProcessed.Remove(studentId);
        }

        private uint[] getFamilyWithStudentsInternal(int numAttempts, Building buildingData) 
        {
            // Check to see if too many attempts already
            if (numAttempts <= 0) 
            {
                return null;
            }

            // Get a random family with students
            uint familyId = this.fetchRandomFamilyWithStudents();
                
            Logger.LogInfo(Logger.LOG_STUDENTS, "StudentManager.getFamilyWithStudentsInternal moving in -- Family Id: {0}", familyId);
            if (familyId == 0) 
            {
                // No Family with students to be located
                return null;
            }

            // validate all students in the family and build an array of family members
            CitizenUnit familyWithStudents = this.citizenManager.m_units.m_buffer[familyId];
            uint[] family = new uint[5];
            bool studentPresent = false;
            for (int i = 0; i < 5; i++) 
            {
                uint familyMember = familyWithStudents.GetCitizen(i);
                if (familyMember != 0 && this.isCampusAreaStudent(familyMember) && this.checkSameCampusArea(familyMember, buildingData)) 
                {
                    Logger.LogInfo(Logger.LOG_STUDENTS, "StudentManager.getFamilyWithStudentsInternal -- Family Member: {0}, is a campus student and can move in", familyMember);
                    if (!this.validateStudent(familyMember)) {
                        // This particular Student is no longer valid for some reason, call recursively with one less attempt
                        return this.getFamilyWithStudentsInternal(--numAttempts, buildingData);
                    }
                    studentPresent = true;
                }
                Logger.LogInfo(Logger.LOG_STUDENTS, "StudentManager.getFamilyWithStudentsInternal -- Family Member: {0}", familyMember);
                family[i] = familyMember;
            }

            if (!studentPresent) 
            {
                // No Student was found in this family (which is a bit weird), try again
                return this.getFamilyWithStudentsInternal(--numAttempts, buildingData);
            }

            return family;
        }

        private uint[] getDormApartmentStudentsInternal(int numAttempts, Building buildingData) 
        {
            // Check to see if too many attempts already
            if (numAttempts <= 0) 
            {
                return null;
            }

            // Get a random dorm apartment
            uint dormApartmentId = this.fetchRandomDormApartment(buildingData);  

            Logger.LogInfo(Logger.LOG_STUDENTS, "StudentManager.getDormApartmentStudentsInternal -- Family Id: {0}", dormApartmentId);
            if (dormApartmentId == 0) 
            {
                // No dorm apartment to be located
                return null;
            }

            // create an array of students to move out of the apartment
            CitizenUnit dormApartment = this.citizenManager.m_units.m_buffer[dormApartmentId];
            uint[] dorm_apartment = new uint[] {0, 0, 0, 0, 0};
            for (int i = 0; i < 5; i++) 
            {
                uint studentId = dormApartment.GetCitizen(i);
                Logger.LogInfo(Logger.LOG_STUDENTS, "StudentManager.getDormApartmentStudentsInternal -- Family Member: {0}", studentId);
                // not a campus area student or this campus area student -> move out
                if(studentId != 0 && !this.isCampusAreaStudent(studentId) || !this.checkSameCampusArea(studentId, buildingData))
                {
                    if (!this.validateStudent(studentId)) {
                        // This particular student is already being processed
                        return this.getDormApartmentStudentsInternal(--numAttempts, buildingData);
                    }
                    Logger.LogInfo(Logger.LOG_STUDENTS, "StudentManager.getDormApartmentStudentsInternal -- Family Member: {0}, is not a student or does not study in this campus", studentId);
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

        private uint fetchRandomDormApartment(Building buildingData) 
        {
            if(buildingData.Info.GetAI() is DormsAI dormsAI)
            {
                if(dormsAI.m_campusType == DistrictPark.ParkType.University)
                {
                    if (this.numStudentsMoveOutUniversity <= 0) 
                    {
                        return 0;
                    }

                    int index = this.randomizer.Int32(this.numStudentsMoveOutUniversity);
                    return this.studentsMovingOutUniversity[index];
                }
                if(dormsAI.m_campusType == DistrictPark.ParkType.LiberalArts)
                {
                    if (this.numStudentsMoveOutLiberalArts <= 0) 
                    {
                        return 0;
                    }

                    int index = this.randomizer.Int32(this.numStudentsMoveOutLiberalArts);
                    return this.studentsMovingOutLiberalArts[index];
                }
                if(dormsAI.m_campusType == DistrictPark.ParkType.TradeSchool)
                {
                    if (this.numStudentsMoveOutTradeSchool <= 0) 
                    {
                        return 0;
                    }

                    int index = this.randomizer.Int32(this.numStudentsMoveOutTradeSchool);
                    return this.studentsMovingOutTradeSchool[index];
                }
            }
            
            return 0;
        }

        private bool checkSameCampusArea(uint studentId, Building buildingData)
        {
            ushort studyBuildingId = this.citizenManager.m_citizens.m_buffer[studentId].m_workBuilding;
            Building studyBuilding = buildingManager.m_buildings.m_buffer[studyBuildingId];

            DistrictManager districtManager = Singleton<DistrictManager>.instance;

            var dorms_park = districtManager.GetPark(buildingData.m_position); // dorms campus park according to dorms position
            var study_park = districtManager.GetPark(studyBuilding.m_position); // student campus park according to workplace position

            // student is studying in the same campus area that he lives in -- position and campus type
            if(buildingData.Info.m_buildingAI is DormsAI dorms && dorms_park == study_park)
            {
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
                return false; // being processed 
            }

            return true; // not being processed
        }

        public bool isCampusAreaStudent(uint citizenId)
        {
            if (citizenId == 0) 
            {
                return false;
            }

            // Validate not dead
            if (this.citizenManager.m_citizens.m_buffer[citizenId].Dead) 
            {
                return false;
            }

            ushort studyBuildingId = this.citizenManager.m_citizens.m_buffer[citizenId].m_workBuilding;
            Building studyBuilding = buildingManager.m_buildings.m_buffer[studyBuildingId];

            // Validate studying in a campus area
            if(studyBuilding.Info.m_buildingAI is not CampusBuildingAI && studyBuilding.Info.m_buildingAI is not MainCampusBuildingAI)
            {
                 return false;
            }

             // Validate is a student
            if ((this.citizenManager.m_citizens.m_buffer[citizenId].m_flags & Citizen.Flags.Student) == 0) 
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

            // if already living in a dorm
            if(homeBuilding.Info.m_buildingAI is DormsAI)
            {
                return false;
            } 

            // not studying in a campus area
            if(!this.isCampusAreaStudent(citizenId))
            {
                return false;
            }

            return true;
        }

        private bool isMovingOutUniversity(uint citizenId)
        {
            // if this student is living in the dorms we should check the entire apartment
            ushort homeBuildingId = this.citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
            Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];
            if(homeBuilding.Info.m_buildingAI is DormsAI dormsAI && dormsAI.m_campusType == DistrictPark.ParkType.University)
            {
                return true;
            }

            return false;
        }

        private bool isMovingOutLiberalArts(uint citizenId)
        {
            // if this student is living in the dorms we should check the entire apartment
            ushort homeBuildingId = this.citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
            Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];
            if(homeBuilding.Info.m_buildingAI is DormsAI dormsAI && dormsAI.m_campusType == DistrictPark.ParkType.LiberalArts)
            {
                return true;
            }

            return false;
        }

        private bool isMovingOutTradeSchool(uint citizenId)
        {
            // if this student is living in the dorms we should check the entire apartment
            ushort homeBuildingId = this.citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
            Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];
            if(homeBuilding.Info.m_buildingAI is DormsAI dormsAI && dormsAI.m_campusType == DistrictPark.ParkType.TradeSchool)
            {
                return true;
            }

            return false;
        }
    }
}