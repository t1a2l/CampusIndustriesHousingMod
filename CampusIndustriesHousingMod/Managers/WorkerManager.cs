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
            Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager Created");
            instance = this;

            this.randomizer = new Randomizer((uint) 73);
            this.citizenManager = Singleton<CitizenManager>.instance;
            this.buildingManager = Singleton<BuildingManager>.instance;

            this.familiesWithWorkers = [];

            this.farmingBarracksFamilies = [];

            this.forestryBarracksFamilies = [];

            this.oilBarracksFamilies = [];

            this.oreBarracksFamilies = [];

            this.familiesBeingProcessed = [];

        }

        public static WorkerManager getInstance() 
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

            if (Interlocked.CompareExchange(ref this.running, 1, 0) == 1)
            {
                return;
            }

            ushort step = workerCheckStep;
            workerCheckStep = (ushort)((step + 1) & StepMask);

            RefreshWorkers(step);

            this.running = 0;
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

                        if (this.validateFamily(family))
                        {
                            for (int k = 0; k < family.Length; k++)
                            {
                                uint familyMember = family[k];
                                if (familyMember != 0 && this.isMovingIn(familyMember))
                                {
                                    this.familiesWithWorkers.Add(num);
                                    move_in = true; // moving in, so not moving out
                                    break;
                                }
                            }
                            if (!move_in)
                            {
                                if (this.isMovingOutFarming(family))
                                {
                                    this.farmingBarracksFamilies.Add(num);
                                }
                                else if (this.isMovingOutForestry(family))
                                {
                                    this.forestryBarracksFamilies.Add(num);
                                }
                                else if (this.isMovingOutOil(family))
                                {
                                    this.oilBarracksFamilies.Add(num);
                                }
                                else if (this.isMovingOutOre(family))
                                {
                                    this.oreBarracksFamilies.Add(num);
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

        public uint[] getFamilyWithWorkers(Building buildingData) 
        {
            return this.getFamilyWithWorkers(DEFAULT_NUM_SEARCH_ATTEMPTS, buildingData);
        }

        public uint[] getBarracksApartmentFamily(Building buildingData) 
        {
            return this.getBarracksApartmentFamily(DEFAULT_NUM_SEARCH_ATTEMPTS, buildingData);
        }

        public uint[] getFamilyWithWorkers(int numAttempts, Building buildingData) 
        {
            Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.getFamilyWithWorkers -- Start");
            // Lock to prevent refreshing while running, otherwise bail
            if (Interlocked.CompareExchange(ref this.running, 1, 0) == 1) 
            {
                return null;
            }

            // Get random family that contains at least one industry area worker
            uint[] family = this.getFamilyWithWorkersInternal(numAttempts, buildingData);
            if (family == null) 
            {
                Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.getFamilyWithWorkersInternal -- No Family");
                this.running = 0;
                return null;
            }

            this.familiesBeingProcessed.Add(family);

            Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.getFamilyWithWorkersInternal -- Finished: {0}", string.Join(", ", Array.ConvertAll(family, item => item.ToString())));
            this.running = 0;
            return family;
        }

        public uint[] getBarracksApartmentFamily(int numAttempts, Building buildingData) 
        {
            Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.getBarracksApartmentFamily -- Start");
            // Lock to prevent refreshing while running, otherwise bail
            if (Interlocked.CompareExchange(ref this.running, 1, 0) == 1) 
            {
                return null;
            }

            // Get random apartment from the barracks
            uint[] barracks_family = this.getBarracksApartmentFamilyInternal(numAttempts, buildingData);
            if (barracks_family == null) 
            {
                Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.getBarracksApartmentFamily -- No Barracks Family");
                this.running = 0;
                return null;
            }

            this.familiesBeingProcessed.Add(barracks_family);

            Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.getBarracksApartmentFamily -- Finished: {0}", string.Join(", ", Array.ConvertAll(barracks_family, item => item.ToString())));
            this.running = 0;
            return barracks_family;
        }

        public void doneProcessingFamily(uint[] family) 
        {
          this.familiesBeingProcessed.Remove(family);
        }

        private uint[] getFamilyWithWorkersInternal(int numAttempts, Building buildingData) 
        {
            // Check to see if too many attempts already
            if (numAttempts <= 0) 
            {
                return null;
            }

            // Get a random family with workers
            uint familyId = this.fetchRandomFamilyWithWorkers();
                
            Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.getFamilyWithWorkersInternal moving in -- Family Id: {0}", familyId);
            if (familyId == 0) 
            {
                // No Family with workers to be located
                return null;
            }

            // build an array of family members
            CitizenUnit familyWithWorkers = this.citizenManager.m_units.m_buffer[familyId];
            uint[] family = new uint[5];
            bool workerPresent = false;
            for (int i = 0; i < 5; i++) 
            {
                uint familyMember = familyWithWorkers.GetCitizen(i);
                if (familyMember != 0 && this.isIndustryAreaWorker(familyMember) && this.checkSameIndestryArea(familyMember, buildingData)) 
                {
                    Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.getFamilyWithWorkerInternal -- Family Member: {0}, is an industrial worker and can move in", familyMember);
                    workerPresent = true;
                }
                Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.getFamilyWithWorkerInternal -- Family Member: {0}", familyMember);
                family[i] = familyMember;
            }

            if (!this.validateFamily(family)) 
            {
                // This particular family is already being proccesed 
                return this.getFamilyWithWorkersInternal(--numAttempts, buildingData);
            }

            if (!workerPresent) 
            {
                // No Worker was found in this family (which is a bit weird), try again
                return this.getFamilyWithWorkersInternal(--numAttempts, buildingData);
            }

            return family;
        }

        private uint[] getBarracksApartmentFamilyInternal(int numAttempts, Building buildingData) 
        {
            // Check to see if too many attempts already
            if (numAttempts <= 0) 
            {
                return null;
            }

            // Get a random barracks apartment according to the barracks industry type
            uint barracksApartmentId = this.fetchRandomBarracksApartment(buildingData);  

            Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.getBarracksApartmentFamilyInternal -- Family Id: {0}", barracksApartmentId);
            if (barracksApartmentId == 0) 
            {
                // No barracks apartment to be located
                return null;
            }

            // create an array of the barracks family to move out of the apartment
            CitizenUnit barracksApartment = this.citizenManager.m_units.m_buffer[barracksApartmentId];
            uint[] barracks_apartment = new uint[] {0, 0, 0, 0, 0};
            for (int i = 0; i < 5; i++) 
            {
                uint familyMember = barracksApartment.GetCitizen(i);
                Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.getBarracksApartmentFamilyInternal -- Family Member: {0}", familyMember);
                // found a worker in the family -> no need to move out
                if (familyMember != 0 && this.isIndustryAreaWorker(familyMember) && this.checkSameIndestryArea(familyMember, buildingData))
                {
                    Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.getBarracksApartmentFamilyInternal -- Family Member: {0}, is a worker", familyMember);
                    return null;
                } 
                barracks_apartment[i] = familyMember;
            }

            if (!this.validateFamily(barracks_apartment)) 
            {
                // This particular family is already being proccesed 
                return this.getBarracksApartmentFamilyInternal(--numAttempts, buildingData);
            }

            return barracks_apartment;
        
        }

        private uint fetchRandomFamilyWithWorkers() 
        {
            if (this.familiesWithWorkers.Count == 0) 
            {
                return 0;
            }

            int index = this.randomizer.Int32((uint)this.familiesWithWorkers.Count);
            var family = this.familiesWithWorkers[index];
            this.familiesWithWorkers.RemoveAt(index);
            return family;
        }

        private uint fetchRandomBarracksApartment(Building buildingData) 
        {
            if(buildingData.Info.GetAI() is BarracksAI barracksAI)
            {
                if(barracksAI.m_industryType == DistrictPark.ParkType.Farming)
                {
                    if (this.farmingBarracksFamilies.Count == 0)
                    {
                        return 0;
                    }

                    int index = this.randomizer.Int32((uint)this.farmingBarracksFamilies.Count);
                    var family = this.farmingBarracksFamilies[index];
                    this.farmingBarracksFamilies.RemoveAt(index);
                    return family;
                }
                if(barracksAI.m_industryType == DistrictPark.ParkType.Forestry)
                {
                    if (this.forestryBarracksFamilies.Count == 0)
                    {
                        return 0;
                    }

                    int index = this.randomizer.Int32((uint)this.forestryBarracksFamilies.Count);
                    var family = this.forestryBarracksFamilies[index];
                    this.forestryBarracksFamilies.RemoveAt(index);
                    return family;
                }
                if(barracksAI.m_industryType == DistrictPark.ParkType.Oil)
                {
                    if (this.oilBarracksFamilies.Count == 0)
                    {
                        return 0;
                    }

                    int index = this.randomizer.Int32((uint)this.oilBarracksFamilies.Count);
                    var family = this.oilBarracksFamilies[index];
                    this.oilBarracksFamilies.RemoveAt(index);
                    return family;
                }
                if(barracksAI.m_industryType == DistrictPark.ParkType.Ore)
                {
                    if (this.oreBarracksFamilies.Count == 0)
                    {
                        return 0;
                    }

                    int index = this.randomizer.Int32((uint)this.oreBarracksFamilies.Count);
                    var family = this.oreBarracksFamilies[index];
                    this.oreBarracksFamilies.RemoveAt(index);
                    return family;
                }
            } 
         
            return 0;
        }

        private bool checkSameIndestryArea(uint workerId, Building buildingData)
        {
            ushort workBuildingId = this.citizenManager.m_citizens.m_buffer[workerId].m_workBuilding;
            Building workBuilding = buildingManager.m_buildings.m_buffer[workBuildingId];

            DistrictManager districtManager = Singleton<DistrictManager>.instance;

            var barracks_park = districtManager.GetPark(buildingData.m_position); // barracks industry park according to barracks position
            var workplace_park = districtManager.GetPark(workBuilding.m_position); // workplace industry park according to workplace position

            Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.checkIndestryArea -- barracks: {0}", buildingData.Info.name);
            Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.checkIndestryArea -- work place: {0}", workBuilding.Info.name);

            // woker is working in the same industrial area that he lives in -- position and insudtry type
            if(buildingData.Info.m_buildingAI is BarracksAI barracks && barracks_park == workplace_park)
            {
                Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.checkIndestryArea -- same industry park");
                Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.checkIndestryArea -- industry Type: {0}", barracks.m_industryType.ToString());

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

            Logger.LogInfo(Logger.LOG_WORKERS, "WorkerManager.checkIndestryArea -- not working in the same industry area or not industry worker");

            return false;
        }

        private bool validateFamily(uint[] family)
        {
            // Validate this family is not already being processed
            if (this.familiesBeingProcessed.Contains(family)) 
            {
                return false; // being processed 
            }

            return true; // not being processed
        }

        public bool isIndustryAreaWorker(uint citizenId)
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

            ushort workBuildingId = this.citizenManager.m_citizens.m_buffer[citizenId].m_workBuilding;
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

        private bool isMovingIn(uint citizenId)
        {
            ushort homeBuildingId = this.citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
            ushort workBuildingId = this.citizenManager.m_citizens.m_buffer[citizenId].m_workBuilding;

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
            if(!this.isIndustryAreaWorker(citizenId))
            {
                return false;
            }

            return true;
        }

        private bool isMovingOutFarming(uint[] citizen_family)
        {
            // if this family is living in the barracks there are up to moveout
            for (int i = 0; i < citizen_family.Length; i++) 
            {
                var citizenId = citizen_family[i];
                if(citizenId != 0)
                {
                    ushort homeBuildingId = this.citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
                    Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];
                    if(homeBuilding.Info.m_buildingAI is BarracksAI barracksAI && barracksAI.m_industryType == DistrictPark.ParkType.Farming)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool isMovingOutForestry(uint[] citizen_family)
        {
            // if this family is living in the barracks there are up to moveout
            for (int i = 0; i < citizen_family.Length; i++) 
            {
                var citizenId = citizen_family[i];
                if(citizenId != 0)
                {
                    ushort homeBuildingId = this.citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
                    Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];
                    if(homeBuilding.Info.m_buildingAI is BarracksAI barracksAI && barracksAI.m_industryType == DistrictPark.ParkType.Forestry)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool isMovingOutOil(uint[] citizen_family)
        {
            // if this family is living in the barracks there are up to moveout
            for (int i = 0; i < citizen_family.Length; i++) 
            {
                var citizenId = citizen_family[i];
                if(citizenId != 0)
                {
                    ushort homeBuildingId = this.citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
                    Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];
                    if(homeBuilding.Info.m_buildingAI is BarracksAI barracksAI && barracksAI.m_industryType == DistrictPark.ParkType.Oil)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool isMovingOutOre(uint[] citizen_family)
        {
            // if this family is living in the barracks there are up to moveout
            for (int i = 0; i < citizen_family.Length; i++) 
            {
                var citizenId = citizen_family[i];
                if(citizenId != 0)
                {
                    ushort homeBuildingId = this.citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
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