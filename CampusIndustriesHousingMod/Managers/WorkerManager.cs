using System;
using System.Collections.Generic;
using System.Threading;
using CampusIndustriesHousingMod.AI;
using ColossalFramework;
using ColossalFramework.Math;
using ICities;

namespace CampusIndustriesHousingMod.Managers
{
    public class WorkerManager : ThreadingExtensionBase 
    {
        private const int DEFAULT_NUM_SEARCH_ATTEMPTS = 3;

        private static WorkerManager instance;

        private readonly BuildingManager buildingManager;
        private readonly CitizenManager citizenManager;

        private readonly List<uint> familiesWithWorkers;
        private readonly List<uint> farmingBarracksFamilies;
        private readonly List<uint> forestryBarracksFamilies;
        private readonly List<uint> oilBarracksFamilies;
        private readonly List<uint> oreBarracksFamilies;

        private readonly HashSet<uint[]> familiesBeingProcessed;

        private Randomizer randomizer;

        private int running;

        private const int StepMask = 0xFF;
        private const int BuildingStepSize = 192;
        private ushort workerCheckStep;

        private int workerCheckCounter;

        public WorkerManager() 
        {
            Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager Created");
            instance = this;

            randomizer = new Randomizer((uint) 73);
            citizenManager = Singleton<CitizenManager>.instance;
            buildingManager = Singleton<BuildingManager>.instance;

            familiesWithWorkers = [];

            farmingBarracksFamilies = [];

            forestryBarracksFamilies = [];

            oilBarracksFamilies = [];

            oreBarracksFamilies = [];

            familiesBeingProcessed = [];

        }

        public static WorkerManager GetInstance() 
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
            RefreshWorkers();

            if ((frameIndex & StepMask) != 0)
            {
                return;
            }
        }

        private void RefreshWorkers()
        {
            if (workerCheckCounter > 0)
            {
                --workerCheckCounter;
                return;
            }

            if (Interlocked.CompareExchange(ref running, 1, 0) == 1)
            {
                return;
            }

            ushort step = workerCheckStep;
            workerCheckStep = (ushort)((step + 1) & StepMask);

            RefreshWorkers(step);

            running = 0;
        }

        private void RefreshWorkers(uint step) 
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint[] family;

            ushort first = (ushort)(step * BuildingStepSize);
            ushort last = (ushort)((step + 1) * BuildingStepSize - 1);

            for (ushort i = first; i <= last; ++i)
            {
                var building = buildingManager.m_buildings.m_buffer[i];
                if(building.Info == null || (building.Info.GetAI() is not ResidentialBuildingAI && building.Info.GetAI() is not BarracksAI))
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
                        family = new uint[5];
                        for (int j = 0; j < 5; j++)
                        {
                            uint citizenId = citizenUnit.GetCitizen(j);
                            if (citizenManager.m_citizens.m_buffer[citizenId].m_flags.IsFlagSet(Citizen.Flags.Created))
                            {
                                family[j] = citizenId;
                            }
                        }

                        bool move_in = false;

                        if (ValidateFamily(family))
                        {
                            for (int k = 0; k < family.Length; k++)
                            {
                                uint familyMember = family[k];
                                if (familyMember != 0 && IsMovingIn(familyMember))
                                {
                                    familiesWithWorkers.Add(num);
                                    move_in = true; // moving in, so not moving out
                                    break;
                                }
                            }
                            if (!move_in)
                            {
                                if (IsMovingOutFarming(family))
                                {
                                    farmingBarracksFamilies.Add(num);
                                }
                                else if (IsMovingOutForestry(family))
                                {
                                    forestryBarracksFamilies.Add(num);
                                }
                                else if (IsMovingOutOil(family))
                                {
                                    oilBarracksFamilies.Add(num);
                                }
                                else if (IsMovingOutOre(family))
                                {
                                    oreBarracksFamilies.Add(num);
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

        public uint[] GetFamilyWithWorkers(Building buildingData) 
        {
            return GetFamilyWithWorkers(DEFAULT_NUM_SEARCH_ATTEMPTS, buildingData);
        }

        public uint[] GetBarracksApartmentFamily(Building buildingData) 
        {
            return GetBarracksApartmentFamily(DEFAULT_NUM_SEARCH_ATTEMPTS, buildingData);
        }

        public uint[] GetFamilyWithWorkers(int numAttempts, Building buildingData) 
        {
            Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.GetFamilyWithWorkers -- Start");
            // Lock to prevent refreshing while running, otherwise bail
            if (Interlocked.CompareExchange(ref running, 1, 0) == 1) 
            {
                return null;
            }

            // Get random family that contains at least one industry area worker
            uint[] family = GetFamilyWithWorkersInternal(numAttempts, buildingData);
            if (family == null) 
            {
                Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.GetFamilyWithWorkersInternal -- No Family");
                running = 0;
                return null;
            }

            familiesBeingProcessed.Add(family);

            Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.GetFamilyWithWorkersInternal -- Finished: {0}", string.Join(", ", Array.ConvertAll(family, item => item.ToString())));
            running = 0;
            return family;
        }

        public uint[] GetBarracksApartmentFamily(int numAttempts, Building buildingData) 
        {
            Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.GetBarracksApartmentFamily -- Start");
            // Lock to prevent refreshing while running, otherwise bail
            if (Interlocked.CompareExchange(ref running, 1, 0) == 1) 
            {
                return null;
            }

            // Get random apartment from the barracks
            uint[] barracks_family = GetBarracksApartmentFamilyInternal(numAttempts, buildingData);
            if (barracks_family == null) 
            {
                Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.GetBarracksApartmentFamily -- No Barracks Family");
                running = 0;
                return null;
            }

            familiesBeingProcessed.Add(barracks_family);

            Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.GetBarracksApartmentFamily -- Finished: {0}", string.Join(", ", Array.ConvertAll(barracks_family, item => item.ToString())));
            running = 0;
            return barracks_family;
        }

        public void DoneProcessingFamily(uint[] family) 
        {
          familiesBeingProcessed.Remove(family);
        }

        private uint[] GetFamilyWithWorkersInternal(int numAttempts, Building buildingData) 
        {
            // Check to see if too many attempts already
            if (numAttempts <= 0) 
            {
                return null;
            }

            // Get a random family with workers
            uint familyId = FetchRandomFamilyWithWorkers();
                
            Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.GetFamilyWithWorkersInternal moving in -- Family Id: {0}", familyId);
            if (familyId == 0) 
            {
                // No Family with workers to be located
                return null;
            }

            // build an array of family members
            CitizenUnit familyWithWorkers = citizenManager.m_units.m_buffer[familyId];
            uint[] family = new uint[5];
            bool workerPresent = false;
            for (int i = 0; i < 5; i++) 
            {
                uint familyMember = familyWithWorkers.GetCitizen(i);
                if (familyMember != 0 && IsIndustryAreaWorker(familyMember) && CheckSameIndestryArea(familyMember, buildingData)) 
                {
                    Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.GetFamilyWithWorkerInternal -- Family Member: {0}, is an industrial worker and can move in", familyMember);
                    workerPresent = true;
                }
                Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.GetFamilyWithWorkerInternal -- Family Member: {0}", familyMember);
                family[i] = familyMember;
            }

            if (!ValidateFamily(family)) 
            {
                // This particular family is already being proccesed 
                return GetFamilyWithWorkersInternal(--numAttempts, buildingData);
            }

            if (!workerPresent) 
            {
                // No Worker was found in this family (which is a bit weird), try again
                return GetFamilyWithWorkersInternal(--numAttempts, buildingData);
            }

            return family;
        }

        private uint[] GetBarracksApartmentFamilyInternal(int numAttempts, Building buildingData) 
        {
            // Check to see if too many attempts already
            if (numAttempts <= 0) 
            {
                return null;
            }

            // Get a random barracks apartment according to the barracks industry type
            uint barracksApartmentId = FetchRandomBarracksApartment(buildingData);  

            Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.GetBarracksApartmentFamilyInternal -- Family Id: {0}", barracksApartmentId);
            if (barracksApartmentId == 0) 
            {
                // No barracks apartment to be located
                return null;
            }

            // create an array of the barracks family to move out of the apartment
            CitizenUnit barracksApartment = citizenManager.m_units.m_buffer[barracksApartmentId];
            uint[] barracks_apartment = new uint[] {0, 0, 0, 0, 0};
            for (int i = 0; i < 5; i++) 
            {
                uint familyMember = barracksApartment.GetCitizen(i);
                Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.GetBarracksApartmentFamilyInternal -- Family Member: {0}", familyMember);
                // found a worker in the family -> no need to move out
                if (familyMember != 0 && IsIndustryAreaWorker(familyMember) && CheckSameIndestryArea(familyMember, buildingData))
                {
                    Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.GetBarracksApartmentFamilyInternal -- Family Member: {0}, is a worker", familyMember);
                    return null;
                } 
                barracks_apartment[i] = familyMember;
            }

            if (!ValidateFamily(barracks_apartment)) 
            {
                // This particular family is already being proccesed 
                return GetBarracksApartmentFamilyInternal(--numAttempts, buildingData);
            }

            return barracks_apartment;
        
        }

        private uint FetchRandomFamilyWithWorkers() 
        {
            if (familiesWithWorkers.Count == 0) 
            {
                return 0;
            }

            int index = randomizer.Int32((uint)familiesWithWorkers.Count);
            var family = familiesWithWorkers[index];
            familiesWithWorkers.RemoveAt(index);
            return family;
        }

        private uint FetchRandomBarracksApartment(Building buildingData) 
        {
            if(buildingData.Info.GetAI() is BarracksAI barracksAI)
            {
                if(barracksAI.m_industryType == DistrictPark.ParkType.Farming)
                {
                    if (farmingBarracksFamilies.Count == 0)
                    {
                        return 0;
                    }

                    int index = randomizer.Int32((uint)farmingBarracksFamilies.Count);
                    var family = farmingBarracksFamilies[index];
                    farmingBarracksFamilies.RemoveAt(index);
                    return family;
                }
                if(barracksAI.m_industryType == DistrictPark.ParkType.Forestry)
                {
                    if (forestryBarracksFamilies.Count == 0)
                    {
                        return 0;
                    }

                    int index = randomizer.Int32((uint)forestryBarracksFamilies.Count);
                    var family = forestryBarracksFamilies[index];
                    forestryBarracksFamilies.RemoveAt(index);
                    return family;
                }
                if(barracksAI.m_industryType == DistrictPark.ParkType.Oil)
                {
                    if (oilBarracksFamilies.Count == 0)
                    {
                        return 0;
                    }

                    int index = randomizer.Int32((uint)oilBarracksFamilies.Count);
                    var family = oilBarracksFamilies[index];
                    oilBarracksFamilies.RemoveAt(index);
                    return family;
                }
                if(barracksAI.m_industryType == DistrictPark.ParkType.Ore)
                {
                    if (oreBarracksFamilies.Count == 0)
                    {
                        return 0;
                    }

                    int index = randomizer.Int32((uint)oreBarracksFamilies.Count);
                    var family = oreBarracksFamilies[index];
                    oreBarracksFamilies.RemoveAt(index);
                    return family;
                }
            } 
         
            return 0;
        }

        private bool CheckSameIndestryArea(uint workerId, Building buildingData)
        {
            ushort workBuildingId = citizenManager.m_citizens.m_buffer[workerId].m_workBuilding;
            Building workBuilding = buildingManager.m_buildings.m_buffer[workBuildingId];

            DistrictManager districtManager = Singleton<DistrictManager>.instance;

            var barracks_park = districtManager.GetPark(buildingData.m_position); // barracks industry park according to barracks position
            var workplace_park = districtManager.GetPark(workBuilding.m_position); // workplace industry park according to workplace position

            Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.CheckIndestryArea -- barracks: {0}", buildingData.Info.name);
            Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.CheckIndestryArea -- work place: {0}", workBuilding.Info.name);

            // woker is working in the same industrial area that he lives in -- position and insudtry type
            if(buildingData.Info.m_buildingAI is BarracksAI barracks && barracks_park == workplace_park)
            {
                Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.CheckIndestryArea -- same industry park");
                Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.CheckIndestryArea -- industry Type: {0}", barracks.m_industryType.ToString());

                if(workBuilding.Info.m_buildingAI is BarracksAI barracksBuilding && barracks.m_industryType == barracksBuilding.m_industryType)
                {
                    return true;
                }
                if(workBuilding.Info.m_buildingAI is IndustryBuildingAI industryBuilding && barracks.m_industryType == industryBuilding.m_industryType)
                {
                    return true;
                }
                if(workBuilding.Info.m_buildingAI is AuxiliaryBuildingAI auxiliaryBuilding && barracks.m_industryType == auxiliaryBuilding.m_industryType)
                {
                    return true;
                }
                if(workBuilding.Info.m_buildingAI is ExtractingFacilityAI extractingFacility && barracks.m_industryType == extractingFacility.m_industryType)
                {
                    return true;
                }
                if(workBuilding.Info.m_buildingAI is ProcessingFacilityAI processingFacility && barracks.m_industryType == processingFacility.m_industryType)
                {
                    return true;
                }
                if(workBuilding.Info.m_buildingAI is MainIndustryBuildingAI mainIndustryBuilding && barracks.m_industryType == mainIndustryBuilding.m_industryType)
                {
                    return true;
                }
                if(workBuilding.Info.m_buildingAI is WarehouseAI warehouseAI)
                {
                    if(barracks.m_industryType == DistrictPark.ParkType.Farming && warehouseAI.m_storageType == TransferManager.TransferReason.Grain)
                    {
                        return true;
                    }
                    if(barracks.m_industryType == DistrictPark.ParkType.Forestry && warehouseAI.m_storageType == TransferManager.TransferReason.Logs)
                    {
                        return true;
                    }
                    if(barracks.m_industryType == DistrictPark.ParkType.Ore && warehouseAI.m_storageType == TransferManager.TransferReason.Ore)
                    {
                        return true;
                    }
                    if(barracks.m_industryType == DistrictPark.ParkType.Oil && warehouseAI.m_storageType == TransferManager.TransferReason.Oil)
                    {
                        return true;
                    }
                }
            }

            Logger.LogInfo(Logger.LOG_WORKERS_MANAGER, "WorkerManager.CheckIndestryArea -- not working in the same industry area or not industry worker");

            return false;
        }

        private bool ValidateFamily(uint[] family)
        {
            // Validate this family is not already being processed
            if (familiesBeingProcessed.Contains(family)) 
            {
                return false; // being processed 
            }

            return true; // not being processed
        }

        public bool IsIndustryAreaWorker(uint citizenId)
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

            ushort workBuildingId = citizenManager.m_citizens.m_buffer[citizenId].m_workBuilding;
            Building workBuilding = buildingManager.m_buildings.m_buffer[workBuildingId];

            // Validate working in an industrial area building
            if(workBuilding.Info.m_buildingAI is not IndustryBuildingAI && workBuilding.Info.m_buildingAI is AuxiliaryBuildingAI
                && workBuilding.Info.m_buildingAI is not ExtractingFacilityAI && workBuilding.Info.m_buildingAI is not ProcessingFacilityAI
                && workBuilding.Info.m_buildingAI is not MainIndustryBuildingAI && workBuilding.Info.m_buildingAI is not WarehouseAI)
            {
                 return false;
            }

            // validate if working in a industrial area warehouse type
            if(workBuilding.Info.m_buildingAI is WarehouseAI warehouseAI)
            {
                if(warehouseAI.m_storageType != TransferManager.TransferReason.Grain &&
                   warehouseAI.m_storageType != TransferManager.TransferReason.Logs &&
                   warehouseAI.m_storageType != TransferManager.TransferReason.Ore &&
                   warehouseAI.m_storageType != TransferManager.TransferReason.Oil
                    )
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsMovingIn(uint citizenId)
        {
            ushort homeBuildingId = citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
            ushort workBuildingId = citizenManager.m_citizens.m_buffer[citizenId].m_workBuilding;

            // no home or no work
            if (homeBuildingId == 0 || workBuildingId == 0) 
            {
                return false;
            }

            Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];

            // if already living in a barracks
            if(homeBuilding.Info.m_buildingAI is BarracksAI)
            {
                return false;
            } 

            // not an industry area worker
            if(!IsIndustryAreaWorker(citizenId))
            {
                return false;
            }

            return true;
        }

        private bool IsMovingOutFarming(uint[] citizen_family)
        {
            // if this family is living in the barracks there are up to moveout
            for (int i = 0; i < citizen_family.Length; i++) 
            {
                var citizenId = citizen_family[i];
                if(citizenId != 0)
                {
                    ushort homeBuildingId = citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
                    Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];
                    if(homeBuilding.Info.m_buildingAI is BarracksAI barracksAI && barracksAI.m_industryType == DistrictPark.ParkType.Farming)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsMovingOutForestry(uint[] citizen_family)
        {
            // if this family is living in the barracks there are up to moveout
            for (int i = 0; i < citizen_family.Length; i++) 
            {
                var citizenId = citizen_family[i];
                if(citizenId != 0)
                {
                    ushort homeBuildingId = citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
                    Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];
                    if(homeBuilding.Info.m_buildingAI is BarracksAI barracksAI && barracksAI.m_industryType == DistrictPark.ParkType.Forestry)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsMovingOutOil(uint[] citizen_family)
        {
            // if this family is living in the barracks there are up to moveout
            for (int i = 0; i < citizen_family.Length; i++) 
            {
                var citizenId = citizen_family[i];
                if(citizenId != 0)
                {
                    ushort homeBuildingId = citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
                    Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];
                    if(homeBuilding.Info.m_buildingAI is BarracksAI barracksAI && barracksAI.m_industryType == DistrictPark.ParkType.Oil)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsMovingOutOre(uint[] citizen_family)
        {
            // if this family is living in the barracks there are up to moveout
            for (int i = 0; i < citizen_family.Length; i++) 
            {
                var citizenId = citizen_family[i];
                if(citizenId != 0)
                {
                    ushort homeBuildingId = citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
                    Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];
                    if(homeBuilding.Info.m_buildingAI is BarracksAI barracksAI && barracksAI.m_industryType == DistrictPark.ParkType.Ore)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}