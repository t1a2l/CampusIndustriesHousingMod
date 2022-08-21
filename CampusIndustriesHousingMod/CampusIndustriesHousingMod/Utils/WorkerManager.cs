using System;
using System.Collections.Generic;
using System.Threading;
using ColossalFramework;
using ColossalFramework.Math;
using ICities;

namespace CampusIndustriesHousingMod 
{
    public class WorkerManager : ThreadingExtensionBase 
    {
        private const bool LOG_WORKERS = true;

        private const int DEFAULT_NUM_SEARCH_ATTEMPTS = 3;

        private static WorkerManager instance;

        private readonly BuildingManager buildingManager;
        private readonly CitizenManager citizenManager;

        private readonly uint[] familiesWithWorkers;
        private readonly uint[] barracksFamilies;
        private readonly HashSet<uint[]> familiesBeingProcessed;
        private uint numFamiliesWithWorkers;
        private uint numBarracksFamilies;

        private Randomizer randomizer;

        private int refreshTimer;
        private int running;

        public WorkerManager() 
        {
            Logger.logInfo(LOG_WORKERS, "WorkerManager Created");
            instance = this;

            this.randomizer = new Randomizer((uint) 73);
            this.citizenManager = Singleton<CitizenManager>.instance;
            this.buildingManager = Singleton<BuildingManager>.instance;

            uint numCitizenUnits = this.citizenManager.m_units.m_size;

            this.familiesWithWorkers = new uint[numCitizenUnits];

            this.barracksFamilies = new uint[numCitizenUnits];

            this.familiesBeingProcessed = new HashSet<uint[]>();

            this.numFamiliesWithWorkers = 0;

            this.numBarracksFamilies = 0;
        }

        public static WorkerManager getInstance() 
        {
            return instance;
        }

        public override void OnBeforeSimulationTick() 
        {
            // Refresh every every so often
            if (this.refreshTimer++ % 600 == 0) 
            {
                // Make sure refresh can occur, otherwise set the timer so it will trigger again next try
                if (Interlocked.CompareExchange(ref this.running, 1, 0) == 1) 
                {
                    this.refreshTimer = 0;
                    return;
                }

                // Refresh the Workers Array
                this.refreshWorkers();

                // Reset the timer and running flag
                this.refreshTimer = 1;
                this.running = 0;
            }
        }

        private void refreshWorkers() 
        {
            CitizenUnit[] citizenUnits = this.citizenManager.m_units.m_buffer;
            this.numFamiliesWithWorkers = 0;
            this.numBarracksFamilies = 0;
            for (uint i = 0; i < citizenUnits.Length; i++) 
            {
                CitizenUnit citizenUnit = citizenUnits[i];
                uint[] family = new uint[5];
                for (int j = 0; j < 5; j++)
                {
                    uint citizenId = citizenUnit.GetCitizen(j);
                    family[j] = citizenId;
                }

                bool move_in = false;
                if(this.validateFamily(family))
                {
                    for (int k = 0; k < family.Length; k++) 
                    {
                        if (family[k] != 0 && this.isMovingIn(family[k])) 
                        {
                            this.familiesWithWorkers[this.numFamiliesWithWorkers++] = i;
                            move_in = true; // moving in, so not moving out
                            break;  
                        }
                    }
                    if(!move_in && this.isMovingOut(family))
                    {
                        this.barracksFamilies[this.numBarracksFamilies++] = i;
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
            Logger.logInfo(LOG_WORKERS, "WorkerManager.getFamilyWithWorkers -- Start");
            // Lock to prevent refreshing while running, otherwise bail
            if (Interlocked.CompareExchange(ref this.running, 1, 0) == 1) 
            {
                return null;
            }

            // Get random family that contains at least one industry area worker
            uint[] family = this.getFamilyWithWorkersInternal(numAttempts, buildingData);
            if (family == null) 
            {
                Logger.logInfo(LOG_WORKERS, "WorkerManager.getFamilyWithWorkersInternal -- No Family");
                this.running = 0;
                return null;
            }

            Logger.logInfo(LOG_WORKERS, "WorkerManager.getFamilyWithWorkersInternal -- Finished: {0}", string.Join(", ", Array.ConvertAll(family, item => item.ToString())));
            this.running = 0;
            return family;
        }

        public uint[] getBarracksApartmentFamily(int numAttempts, Building buildingData) 
        {
            Logger.logInfo(LOG_WORKERS, "WorkerManager.getBarracksApartmentFamily -- Start");
            // Lock to prevent refreshing while running, otherwise bail
            if (Interlocked.CompareExchange(ref this.running, 1, 0) == 1) 
            {
                return null;
            }

            // Get random apartment from the barracks
            uint[] barracks_family = this.getBarracksApartmentFamilyInternal(numAttempts, buildingData);
            if (barracks_family == null) 
            {
                Logger.logInfo(LOG_WORKERS, "WorkerManager.getBarracksApartmentFamily -- No Barracks Family");
                this.running = 0;
                return null;
            }

            Logger.logInfo(LOG_WORKERS, "WorkerManager.getBarracksApartmentFamily -- Finished: {0}", string.Join(", ", Array.ConvertAll(barracks_family, item => item.ToString())));
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
                
            Logger.logInfo(LOG_WORKERS, "WorkerManager.getFamilyWithWorkersInternal moving in -- Family Id: {0}", familyId);
            if (familyId == 0) 
            {
                // No Family with workers to be located
                return null;
            }

            uint[] family = new uint[5];
            CitizenUnit familyWithWorkers = this.citizenManager.m_units.m_buffer[familyId];
            bool workerPresent = false;
            for (int i = 0; i < 5; i++) 
            {
                uint familyMember = familyWithWorkers.GetCitizen(i);
                if (this.isIndustryAreaWorker(familyMember, buildingData)) 
                {
                    Logger.logInfo(LOG_WORKERS, "WorkerManager.getFamilyWithWorkerInternal -- Family Member: {0}, is industrial worker you can move in", familyMember);
                    workerPresent = true;
                }
                Logger.logInfo(LOG_WORKERS, "WorkerManager.getFamilyWithWorkerInternal -- Family Member: {0}", familyMember);
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

            this.familiesBeingProcessed.Add(family);

            return family;
        }

        private uint[] getBarracksApartmentFamilyInternal(int numAttempts, Building buildingData) 
        {
            // Check to see if too many attempts already
            if (numAttempts <= 0) 
            {
                return null;
            }

            // Get a random barracks apartment
            uint barracksApartmentId = this.fetchRandomBarracksApartment();  

            Logger.logInfo(LOG_WORKERS, "WorkerManager.getBarracksApartmentFamilyInternal -- Family Id: {0}", barracksApartmentId);
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
                Logger.logInfo(LOG_WORKERS, "WorkerManager.getBarracksApartmentFamilyInternal -- Family Member: {0}", familyMember);

                if (familyMember != 0 && this.isIndustryAreaWorker(familyMember, buildingData))
                {
                    Logger.logInfo(LOG_WORKERS, "WorkerManager.getBarracksApartmentFamilyInternal -- Family Member: {0}, is a worker", familyMember);
                    return null;
                }                    
                Logger.logInfo(LOG_WORKERS, "WorkerManager.getBarracksApartmentFamilyInternal -- Family Member: {0}", familyMember);
                barracks_apartment[i] = familyMember;
            }

            if (!this.validateFamily(barracks_apartment)) 
            {
                // This particular family is already being proccesed 
                return this.getBarracksApartmentFamilyInternal(--numAttempts, buildingData);
            }

            this.familiesBeingProcessed.Add(barracks_apartment);

            return barracks_apartment;
        
        }

        private uint fetchRandomFamilyWithWorkers() 
        {
            if (this.numFamiliesWithWorkers <= 0) 
            {
                return 0;
            }

            int index = this.randomizer.Int32(this.numFamiliesWithWorkers);
            return this.familiesWithWorkers[index];
        }

        private uint fetchRandomBarracksApartment() 
        {
            if (this.numBarracksFamilies <= 0) 
            {
                return 0;
            }

            int index = this.randomizer.Int32(this.numBarracksFamilies);
            return this.barracksFamilies[index];
        }

        public bool isIndustryAreaWorker(uint workerId, Building buildingData) 
        {
            if (workerId == 0) 
            {
                return false;
            }

            // Validate not dead
            if (this.citizenManager.m_citizens.m_buffer[workerId].Dead) 
            {
                return false;
            }

            // Validate working in the industrial area and living in a barracks that is located at the same industrial area
            if(!this.checkIndestryArea(workerId, buildingData)) 
            {
                return false;
            }

            return true;
        }

        private bool checkIndestryArea(uint workerId, Building buildingData)
        {
            ushort workBuildingId = this.citizenManager.m_citizens.m_buffer[workerId].m_workBuilding;
            Building workBuilding = buildingManager.m_buildings.m_buffer[workBuildingId];

            DistrictManager districtManager = Singleton<DistrictManager>.instance;

            var barracks_park = districtManager.GetPark(buildingData.m_position); // barracks industry park according to barracks position
            var workplace_park = districtManager.GetPark(workBuilding.m_position); // workplace industry park according to workplace position

            Logger.logInfo(LOG_WORKERS, "WorkerManager.checkIndestryArea -- barracks: {0}", buildingData.Info.name);
            Logger.logInfo(LOG_WORKERS, "WorkerManager.checkIndestryArea -- work place: {0}", workBuilding.Info.name);

            // woker is working in the same industrial area that he lives in -- position and insudtry type
            if(buildingData.Info.m_buildingAI is BarracksAI barracks && barracks_park == workplace_park)
            {
                Logger.logInfo(LOG_WORKERS, "WorkerManager.checkIndestryArea -- same industry park");
                Logger.logInfo(LOG_WORKERS, "WorkerManager.checkIndestryArea -- industry Type: {0}", barracks.m_industryType.ToString());

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

            Logger.logInfo(LOG_WORKERS, "WorkerManager.checkIndestryArea -- not working in the same industry area or not industry worker");

            return false;
        }

        private bool validateFamily(uint[] family)
        {
            // Validate this family is not already being processed
            if (this.familiesBeingProcessed.Contains(family)) 
            {
                return false;
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
            Building workBuilding = buildingManager.m_buildings.m_buffer[workBuildingId];

            // not working in an industry area building
            if(workBuilding.Info.m_buildingAI is not IndustryBuildingAI && workBuilding.Info.m_buildingAI is not AuxiliaryBuildingAI &&
                workBuilding.Info.m_buildingAI is not ExtractingFacilityAI && workBuilding.Info.m_buildingAI is not ProcessingFacilityAI
                && workBuilding.Info.m_buildingAI is not MainIndustryBuildingAI)
            {
                 return false;
            }

            // if already living in a barracks
            if(homeBuilding.Info.m_buildingAI is BarracksAI)
            {
                return false;
            } 

            return true;
        }

        private bool isMovingOut(uint[] citizen_family)
        {
            // if this family is living in the barracks there are up to moveout
            for (int i = 0; i < citizen_family.Length; i++) 
            {
                var citizenId = citizen_family[i];
                if(citizenId != 0)
                {
                    ushort homeBuildingId = this.citizenManager.m_citizens.m_buffer[citizenId].m_homeBuilding;
                    Building homeBuilding = buildingManager.m_buildings.m_buffer[homeBuildingId];
                    if(homeBuilding.Info.m_buildingAI is BarracksAI)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}