using System;
using System.Collections.Generic;
using System.Threading;
using CampusIndustriesHousingMod.AI;
using ColossalFramework;
using ColossalFramework.Math;
using ICities;

namespace CampusIndustriesHousingMod.Managers
{
    public class StudentManager : ThreadingExtensionBase 
    {
        private const int DEFAULT_NUM_SEARCH_ATTEMPTS = 3;

        private static StudentManager instance;

        private readonly BuildingManager buildingManager;
        private readonly CitizenManager citizenManager;

        private readonly List<uint> familiesWithStudents;
        private readonly List<uint> studentsMovingOutUniversity;
        private readonly List<uint> studentsMovingOutLiberalArts;
        private readonly List<uint> studentsMovingOutTradeSchool;

        private readonly HashSet<uint> studentsBeingProcessed;

        private Randomizer randomizer;

        private int running;

        private const int StepMask = 0xFF;
        private const int BuildingStepSize = 192;
        private ushort studentCheckStep;

        private int studentCheckCounter;

        public StudentManager() 
        {
            Logger.LogInfo(Logger.LOG_STUDENTS_MANAGER, "StudentManager Created");
            instance = this;

            randomizer = new Randomizer((uint) 73);
            citizenManager = Singleton<CitizenManager>.instance;
            buildingManager = Singleton<BuildingManager>.instance;

            familiesWithStudents = [];

            studentsMovingOutUniversity = [];

            studentsMovingOutLiberalArts = [];

            studentsMovingOutTradeSchool = [];

            studentsBeingProcessed = [];
        }

        public static StudentManager GetInstance() 
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

            if (Interlocked.CompareExchange(ref running, 1, 0) == 1)
            {
                return;
            }

            ushort step = studentCheckStep;
            studentCheckStep = (ushort)((step + 1) & StepMask);

            RefreshStudents(step);

            running = 0;
        }

        private void RefreshStudents(uint step) 
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            ushort first = (ushort)(step * BuildingStepSize);
            ushort last = (ushort)((step + 1) * BuildingStepSize - 1);

            for (ushort i = first; i <= last; ++i)
            {
                var building = buildingManager.m_buildings.m_buffer[i];
                if (building.Info == null || (building.Info.GetAI() is not ResidentialBuildingAI && building.Info.GetAI() is not DormsAI))
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
                            if (citizen.m_flags.IsFlagSet(Citizen.Flags.Created) && ValidateStudent(citizenId))
                            {
                                if (IsMovingIn(citizenId))
                                {
                                    familiesWithStudents.Add(num);
                                    break;
                                }
                                else if (IsMovingOutUniversity(citizenId))
                                {
                                    studentsMovingOutUniversity.Add(num);
                                    break;
                                }
                                else if (IsMovingOutLiberalArts(citizenId))
                                {
                                    studentsMovingOutLiberalArts.Add(num);
                                    break;
                                }
                                else if (IsMovingOutTradeSchool(citizenId))
                                {
                                    studentsMovingOutTradeSchool.Add(num);
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

        public uint[] GetFamilyWithStudents(Building buildingData) 
        {
            return GetFamilyWithStudents(DEFAULT_NUM_SEARCH_ATTEMPTS, buildingData);
        }

        public uint[] GetDormApartmentStudents(Building buildingData) 
        {
            return GetDormApartmentStudents(DEFAULT_NUM_SEARCH_ATTEMPTS, buildingData);
        }

        public uint[] GetFamilyWithStudents(int numAttempts, Building buildingData) 
        {
            Logger.LogInfo(Logger.LOG_STUDENTS_MANAGER, "StudentManager.getFamilyWithStudents -- Start");
            // Lock to prevent refreshing while running, otherwise bail
            if (Interlocked.CompareExchange(ref running, 1, 0) == 1) 
            {
                return null;
            }

            // Get random family that contains at least one campus area student
            uint[] family = GetFamilyWithStudentsInternal(numAttempts, buildingData);
            if (family == null) 
            {
                Logger.LogInfo(Logger.LOG_STUDENTS_MANAGER, "StudentManager.getFamilyWithStudentsInternal -- No Family");
                running = 0;
                return null;
            }

            // Mark student as being processed
            foreach (uint familyMember in family) 
            {
                if(IsCampusAreaStudent(familyMember))
                {
                    studentsBeingProcessed.Add(familyMember);
                }
            }

            Logger.LogInfo(Logger.LOG_STUDENTS_MANAGER, "StudentManager.getFamilyWithStudents -- Finished: {0}", string.Join(", ", Array.ConvertAll(family, item => item.ToString())));
            running = 0;
            return family;
        }

        public uint[] GetDormApartmentStudents(int numAttempts, Building buildingData) 
        {
            Logger.LogInfo(Logger.LOG_STUDENTS_MANAGER, "StudentManager.getDormApartmentStudents -- Start");
            // Lock to prevent refreshing while running, otherwise bail
            if (Interlocked.CompareExchange(ref running, 1, 0) == 1) 
            {
                return null;
            }

            // Get random apartment from the dorms
            uint[] dorms_apartment = GetDormApartmentStudentsInternal(numAttempts, buildingData);
            if (dorms_apartment == null) 
            {
                Logger.LogInfo(Logger.LOG_STUDENTS_MANAGER, "StudentManager.getDormApartmentStudentsInternal -- No students in this apartment");
                running = 0;
                return null;
            }

            // Mark student as being processed
            foreach (uint student in dorms_apartment) 
            {
                if(IsCampusAreaStudent(student))
                {
                    studentsBeingProcessed.Add(student);
                }
            }

            Logger.LogInfo(Logger.LOG_STUDENTS_MANAGER, "StudentManager.getDormApartmentStudentInternal -- Finished: {0}", string.Join(", ", Array.ConvertAll(dorms_apartment, item => item.ToString())));
            running = 0;
            return dorms_apartment;
        }

        public void DoneProcessingStudent(uint studentId) 
        {
          studentsBeingProcessed.Remove(studentId);
        }

        private uint[] GetFamilyWithStudentsInternal(int numAttempts, Building buildingData) 
        {
            // Check to see if too many attempts already
            if (numAttempts <= 0) 
            {
                return null;
            }

            // Get a random family with students
            uint familyId = FetchRandomFamilyWithStudents();
                
            Logger.LogInfo(Logger.LOG_STUDENTS_MANAGER, "StudentManager.getFamilyWithStudentsInternal moving in -- Family Id: {0}", familyId);
            if (familyId == 0) 
            {
                // No Family with students to be located
                return null;
            }

            // validate all students in the family and build an array of family members
            CitizenUnit familyWithStudents = citizenManager.m_units.m_buffer[familyId];
            uint[] family = new uint[5];
            bool studentPresent = false;
            for (int i = 0; i < 5; i++) 
            {
                uint familyMember = familyWithStudents.GetCitizen(i);
                if (familyMember != 0 && IsCampusAreaStudent(familyMember) && CheckSameCampusArea(familyMember, buildingData)) 
                {
                    Logger.LogInfo(Logger.LOG_STUDENTS_MANAGER, "StudentManager.getFamilyWithStudentsInternal -- Family Member: {0}, is a campus student and can move in", familyMember);
                    if (!ValidateStudent(familyMember)) {
                        // This particular Student is no longer valid for some reason, call recursively with one less attempt
                        return GetFamilyWithStudentsInternal(--numAttempts, buildingData);
                    }
                    studentPresent = true;
                }
                Logger.LogInfo(Logger.LOG_STUDENTS_MANAGER, "StudentManager.getFamilyWithStudentsInternal -- Family Member: {0}", familyMember);
                family[i] = familyMember;
            }

            if (!studentPresent) 
            {
                // No Student was found in this family (which is a bit weird), try again
                return GetFamilyWithStudentsInternal(--numAttempts, buildingData);
            }

            return family;
        }

        private uint[] GetDormApartmentStudentsInternal(int numAttempts, Building buildingData) 
        {
            // Check to see if too many attempts already
            if (numAttempts <= 0) 
            {
                return null;
            }

            // Get a random dorm apartment
            uint dormApartmentId = FetchRandomDormApartment(buildingData);  

            Logger.LogInfo(Logger.LOG_STUDENTS_MANAGER, "StudentManager.getDormApartmentStudentsInternal -- Family Id: {0}", dormApartmentId);
            if (dormApartmentId == 0) 
            {
                // No dorm apartment to be located
                return null;
            }

            // create an array of students to move out of the apartment
            CitizenUnit dormApartment = citizenManager.m_units.m_buffer[dormApartmentId];
            uint[] dorm_apartment = [0, 0, 0, 0, 0];
            for (int i = 0; i < 5; i++) 
            {
                uint studentId = dormApartment.GetCitizen(i);
                Logger.LogInfo(Logger.LOG_STUDENTS_MANAGER, "StudentManager.getDormApartmentStudentsInternal -- Family Member: {0}", studentId);
                // not a campus area student or this campus area student -> move out
                if(studentId != 0 && !IsCampusAreaStudent(studentId) || !CheckSameCampusArea(studentId, buildingData))
                {
                    if (!ValidateStudent(studentId)) {
                        // This particular student is already being processed
                        return GetDormApartmentStudentsInternal(--numAttempts, buildingData);
                    }
                    Logger.LogInfo(Logger.LOG_STUDENTS_MANAGER, "StudentManager.getDormApartmentStudentsInternal -- Family Member: {0}, is not a student or does not study in this campus", studentId);
                    dorm_apartment[i] = studentId;
                } 
            }

            return dorm_apartment;
 
        }

        private uint FetchRandomFamilyWithStudents() 
        {
            if (familiesWithStudents.Count == 0)
            {
                return 0;
            }

            int index = randomizer.Int32((uint)familiesWithStudents.Count);
            var family = familiesWithStudents[index];
            familiesWithStudents.RemoveAt(index);
            return family;
        }

        private uint FetchRandomDormApartment(Building buildingData) 
        {
            if(buildingData.Info.GetAI() is DormsAI dormsAI)
            {
                if(dormsAI.m_campusType == DistrictPark.ParkType.University)
                {
                    if (studentsMovingOutUniversity.Count == 0)
                    {
                        return 0;
                    }

                    int index = randomizer.Int32((uint)studentsMovingOutUniversity.Count);
                    var family = studentsMovingOutUniversity[index];
                    studentsMovingOutUniversity.RemoveAt(index);
                    return family;
                }
                if(dormsAI.m_campusType == DistrictPark.ParkType.LiberalArts)
                {
                    if (studentsMovingOutLiberalArts.Count == 0)
                    {
                        return 0;
                    }

                    int index = randomizer.Int32((uint)studentsMovingOutLiberalArts.Count);
                    var family = studentsMovingOutLiberalArts[index];
                    studentsMovingOutLiberalArts.RemoveAt(index);
                    return family;
                }
                if(dormsAI.m_campusType == DistrictPark.ParkType.TradeSchool)
                {
                    if (studentsMovingOutTradeSchool.Count == 0)
                    {
                        return 0;
                    }

                    int index = randomizer.Int32((uint)studentsMovingOutTradeSchool.Count);
                    var family = studentsMovingOutTradeSchool[index];
                    studentsMovingOutTradeSchool.RemoveAt(index);
                    return family;
                }
            }
            
            return 0;
        }

        private bool CheckSameCampusArea(uint studentId, Building buildingData)
        {
            ushort studyBuildingId = citizenManager.m_citizens.m_buffer[studentId].m_workBuilding;
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

        private bool ValidateStudent(uint studentId) 
        {
            // Validate this Student is not already being processed
            if (studentsBeingProcessed.Contains(studentId)) 
            {
                return false; // being processed 
            }

            return true; // not being processed
        }

        public bool IsCampusAreaStudent(uint citizenId)
        {
            if (citizenId == 0) 
            {
                return false;
            }

            // Validate not dead
            if (citizenManager.m_citizens.m_buffer[citizenId].Dead) 
            {
                return false;
            }

            ushort studyBuildingId = citizenManager.m_citizens.m_buffer[citizenId].m_workBuilding;
            Building studyBuilding = buildingManager.m_buildings.m_buffer[studyBuildingId];

            // Validate studying in a campus area
            if(studyBuilding.Info.m_buildingAI is not CampusBuildingAI && studyBuilding.Info.m_buildingAI is not MainCampusBuildingAI)
            {
                 return false;
            }

             // Validate is a student
            if ((citizenManager.m_citizens.m_buffer[citizenId].m_flags & Citizen.Flags.Student) == 0) 
            {
                return false;
            }

            return true;
        }

        private bool IsMovingIn(uint citizenId)
        {
            ushort homeBuildingId = citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
            ushort studyBuildingId = citizenManager.m_citizens.m_buffer[citizenId].m_workBuilding;

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
            if(!IsCampusAreaStudent(citizenId))
            {
                return false;
            }

            return true;
        }

        private bool IsMovingOutUniversity(uint citizenId)
        {
            // if this student is living in the dorms we should check the entire apartment
            ushort homeBuildingId = citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
            Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];
            if(homeBuilding.Info.m_buildingAI is DormsAI dormsAI && dormsAI.m_campusType == DistrictPark.ParkType.University)
            {
                return true;
            }

            return false;
        }

        private bool IsMovingOutLiberalArts(uint citizenId)
        {
            // if this student is living in the dorms we should check the entire apartment
            ushort homeBuildingId = citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
            Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];
            if(homeBuilding.Info.m_buildingAI is DormsAI dormsAI && dormsAI.m_campusType == DistrictPark.ParkType.LiberalArts)
            {
                return true;
            }

            return false;
        }

        private bool IsMovingOutTradeSchool(uint citizenId)
        {
            // if this student is living in the dorms we should check the entire apartment
            ushort homeBuildingId = citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
            Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];
            if(homeBuilding.Info.m_buildingAI is DormsAI dormsAI && dormsAI.m_campusType == DistrictPark.ParkType.TradeSchool)
            {
                return true;
            }

            return false;
        }
    }
}