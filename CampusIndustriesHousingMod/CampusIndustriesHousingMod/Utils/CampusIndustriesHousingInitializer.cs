using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ColossalFramework;
using UnityEngine;

namespace CampusIndustriesHousingMod
{
    public class CampusIndustriesHousingInitializer : MonoBehaviour {
        private const bool LOG_INITIALIZER = true;

        public const int LOADED_LEVEL_GAME = 6;
        public const int LOADED_LEVEL_ASSET_EDITOR = 19;

        private const String CAMPUS_DORM_NAME = "University Dormitory 01";
        private const String INDUSTRIAL_BARRACKS_NAME = "Farm Workers Barracks 01";

        private static readonly Queue<IEnumerator> ACTION_QUEUE = new Queue<IEnumerator>();
        private static readonly object QUEUE_LOCK = new object();

        private int attemptingDormsInitialization;
        private int attemptingBarracksInitialization;
        private int numTimesSearchedForDorm = 0;
        private int numTimesSearchedForBarracks = 0;

        private bool dormsInitialized;
        private bool barracksInitialized;
        private int numAttempts = 0;
        private int loadedLevel = -1;

        private void Awake() {
            // Specify that this object should not be destroyed
            // Without this statement this object would be cleaned up very quickly
            DontDestroyOnLoad(this);
        }

        private void Start() {
            Logger.logInfo(LOG_INITIALIZER, "CampusIndustriesHousingInitializer Starting");
        }

        public void OnLevelWasLoaded(int level) {
            this.loadedLevel = level;
            Logger.logInfo(LOG_INITIALIZER, "CampusIndustriesHousingInitializer.OnLevelWasLoaded: {0}", level);
        }

        public void OnLevelUnloading() {
            this.loadedLevel = -1;
            Logger.logInfo(LOG_INITIALIZER, "CampusIndustriesHousingInitializer.OnLevelUnloading: {0}", this.loadedLevel);
        }

        public int getLoadedLevel() {
            return this.loadedLevel;
        }

        private void Update() {
            if (!this.dormsInitialized && this.loadedLevel != -1) {
                // Still need initilization, check to see if already attempting initilization
                // Note: Not sure if it's possible for this method to be called more than once at a time, but locking just in case
                if (Interlocked.CompareExchange(ref this.attemptingDormsInitialization, 1, 0) == 0) {
                    this.attemptDormsInitialization();
                }
            }
            if (!this.barracksInitialized && this.loadedLevel != -1) {
                // Still need initilization, check to see if already attempting initilization
                // Note: Not sure if it's possible for this method to be called more than once at a time, but locking just in case
                if (Interlocked.CompareExchange(ref this.attemptingBarracksInitialization, 1, 0) == 0) {
                    this.attemptBarracksInitialization();
                }
            }
        }

        private void attemptDormsInitialization() {
            // Make sure not attempting initilization too many times -- This means the mod may not function properly, but it won't waste resources continuing to try
            if (this.numAttempts++ >= 20) {
                Logger.logError("CampusIndustriesHousingInitializer.attemptDormsInitialization -- *** CAMPUS INDUSTRIES HOUSING FUNCTIONALITY DID NOT INITLIZIE PRIOR TO GAME LOADING -- THE CAMPUS INDUSTRIES HOUSING MOD MAY NOT FUNCTION PROPERLY ***");
                // Set initilized so it won't keep trying
                this.SetDormsInitialized();
            }

            // Check to see if initilization can start
            if (PrefabCollection<BuildingInfo>.LoadedCount() <= 0) {
                this.attemptingDormsInitialization = 0;
                return;
            }

            // Wait for the Campus Dorm to load since all new Dorms awill copy its values
            BuildingInfo dormsBuildingInfo = this.FindDormsBuildingInfo();
            if (dormsBuildingInfo == null) {
                this.attemptingDormsInitialization = 0;
                return;
            }

            // Start loading
            Logger.logInfo(LOG_INITIALIZER, "CampusIndustriesHousingInitializer.attemptInitialization -- Attempting Initialization");
            Singleton<LoadingManager>.instance.QueueLoadingAction(ActionWrapper(() => {
                try {
                    if (this.loadedLevel == LOADED_LEVEL_GAME || this.loadedLevel == LOADED_LEVEL_ASSET_EDITOR) {
                        this.StartCoroutine(this.InitCampusHousing());
                        AddQueuedActionsToLoadingQueue();
                    }
                } catch (Exception e) {
                    Logger.logError("Error loading prefabs: {0}", e.Message);
                }
            }));

            // Set initilized
            this.SetDormsInitialized();
        }

        private void attemptBarracksInitialization() {
            // Make sure not attempting initilization too many times -- This means the mod may not function properly, but it won't waste resources continuing to try
            if (this.numAttempts++ >= 20) {
                Logger.logError("CampusIndustriesHousingInitializer.attemptInitialization -- *** CAMPUS INDUSTRIES HOUSING FUNCTIONALITY DID NOT INITLIZIE PRIOR TO GAME LOADING -- THE CAMPUS INDUSTRIES HOUSING MOD MAY NOT FUNCTION PROPERLY ***");
                // Set initilized so it won't keep trying
                this.SetBarracksInitialized();
            }

            // Check to see if initilization can start
            if (PrefabCollection<BuildingInfo>.LoadedCount() <= 0) {
                this.attemptingBarracksInitialization = 0;
                return;
            }

            // Wait for the Industry Barracks to load since all new Barrackses will copy its values
            BuildingInfo barracksBuildingInfo = this.FindBarracksBuildingInfo();
            if (barracksBuildingInfo == null) {
                this.attemptingBarracksInitialization = 0;
                return;
            }

            // Start loading
            Logger.logInfo(LOG_INITIALIZER, "CampusIndustriesHousingInitializer.attemptInitialization -- Attempting Initialization");
            Singleton<LoadingManager>.instance.QueueLoadingAction(ActionWrapper(() => {
                try {
                    if (this.loadedLevel == LOADED_LEVEL_GAME || this.loadedLevel == LOADED_LEVEL_ASSET_EDITOR) {
                        this.StartCoroutine(this.InitIndustriesHousing());
                        AddQueuedActionsToLoadingQueue();
                    }
                } catch (Exception e) {
                    Logger.logError("Error loading prefabs: {0}", e.Message);
                }
            }));

            // Set initilized
            this.SetBarracksInitialized();
        }

        private void SetDormsInitialized() {
            this.dormsInitialized = true;
            this.attemptingDormsInitialization = 0;
            this.numTimesSearchedForDorm = 0;
        }

        private void SetBarracksInitialized() {
            this.barracksInitialized = true;
            this.attemptingBarracksInitialization = 0;
            this.numTimesSearchedForBarracks = 0;
        }
        
        private BuildingInfo FindDormsBuildingInfo() 
        {
            // First check for the known Campus Dorm
            BuildingInfo campusDormBuildingInfo = PrefabCollection<BuildingInfo>.FindLoaded(CAMPUS_DORM_NAME);
            if (campusDormBuildingInfo != null) {
                return campusDormBuildingInfo;
            }

            // Try 5 times to search for the Campus Dorm before giving up
            if (++this.numTimesSearchedForDorm < 5) {
                return null;
            }

            // Attempt to find a suitable Campus Dorm building that can be used as a template
            Logger.logInfo(LOG_INITIALIZER, "CampusIndustriesHousingInitializer.findCampusDormBuildingInfo -- Couldn't find the Campus Dorm asset after {0} tries, attempting to search for any Building with a CampusBuildingAI", this.numTimesSearchedForDorm);
            for (uint i=0; (long) PrefabCollection<BuildingInfo>.LoadedCount() > (long) i; ++i) {
                BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded(i);
                if (buildingInfo != null && buildingInfo.GetService() == ItemClass.Service.PlayerEducation && !buildingInfo.m_buildingAI.IsWonder() && buildingInfo.m_buildingAI is CampusBuildingAI && buildingInfo.name.Contains("Dormitory")) {
                    Logger.logInfo(LOG_INITIALIZER, "NursingHomeInitializer.findMedicalBuildingInfo -- Using the {0} as a template instead of the Elder Care", buildingInfo);
                    return buildingInfo;
                }
            }

            // Return null to try again next time
            return null;
        }

        private BuildingInfo FindBarracksBuildingInfo() 
        {
            // First check for the known Industrial Barracks
            BuildingInfo industrialBarracksBuildingInfo = PrefabCollection<BuildingInfo>.FindLoaded(INDUSTRIAL_BARRACKS_NAME);
            if (industrialBarracksBuildingInfo != null) {
                return industrialBarracksBuildingInfo;
            }

            // Try 5 times to search for the Industry Barracks before giving up
            if (++this.numTimesSearchedForBarracks < 5) {
                return null;
            }

            // Attempt to find a suitable Industrial Barracks building that can be used as a template
            Logger.logInfo(LOG_INITIALIZER, "CampusIndustriesHousingInitializer.findIndustrialBarracksBuildingInfo -- Couldn't find the Industrial Barracks asset after {0} tries, attempting to search for any Building with a AuxiliaryBuildingAI", this.numTimesSearchedForBarracks);
            for (uint i=0; (long) PrefabCollection<BuildingInfo>.LoadedCount() > (long) i; ++i) {
                BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded(i);
                if (buildingInfo != null && buildingInfo.GetService() == ItemClass.Service.PlayerIndustry && !buildingInfo.m_buildingAI.IsWonder() && buildingInfo.m_buildingAI is AuxiliaryBuildingAI && buildingInfo.name.Contains("Barracks")) {
                    Logger.logInfo(LOG_INITIALIZER, "NursingHomeInitializer.findMedicalBuildingInfo -- Using the {0} as a template instead of the Elder Care", buildingInfo);
                    return buildingInfo;
                }
            }

            // Return null to try again next time
            return null;
        }

        private IEnumerator InitCampusHousing() {
            Logger.logInfo(LOG_INITIALIZER, "CampusIndustriesHousingInitializer.initCampusHousing");
            float capcityModifier = CampusIndustriesHousingMod.getInstance().getOptionsManager().getDormsCapacityModifier();
            uint index = 0U;
            int i = 0;
            BuildingInfo campusDormsBuildingInfo = this.FindDormsBuildingInfo();
            while (!Singleton<LoadingManager>.instance.m_loadingComplete || i++ < 2) {
                Logger.logInfo(LOG_INITIALIZER, "CampusIndustriesHousingInitializer.initCampusHousing -- Iteration: {0}", i);
                for (; PrefabCollection<BuildingInfo>.LoadedCount() > index; ++index) {
                    BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded(index);

                    // Check for replacement of AI
                    if (buildingInfo != null && buildingInfo.name.Contains("Dormitory"))
                    {
                        // custom assets get the university dormitory info
                        if(buildingInfo.m_isCustomContent)
                        {
                            buildingInfo.m_class = campusDormsBuildingInfo.m_class;
                        }
                        AiReplacementHelper.ApplyNewAIToBuilding(buildingInfo, "Dorms");
                    }

                    // Check for updating capacity - Existing CHs will be updated on-load, this will set the data used for placing new homes
                    if (this.loadedLevel == LOADED_LEVEL_GAME && buildingInfo != null && buildingInfo.m_buildingAI is DormsAI dormsAI) 
                    {
                        dormsAI.updateCapacity(capcityModifier);  
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator InitIndustriesHousing() {
            Logger.logInfo(LOG_INITIALIZER, "CampusIndustriesHousingInitializer.initIndustriesHousing");
            float capcityModifier = CampusIndustriesHousingMod.getInstance().getOptionsManager().getBarracksCapacityModifier();
            uint index = 0U;
            int i = 0;
            BuildingInfo industriesHousingBuildingInfo = this.FindDormsBuildingInfo();
            while (!Singleton<LoadingManager>.instance.m_loadingComplete || i++ < 2) {
                Logger.logInfo(LOG_INITIALIZER, "CampusIndustriesHousingInitializer.initIndustriesHousing -- Iteration: {0}", i);
                for (; PrefabCollection<BuildingInfo>.LoadedCount() > index; ++index) {
                    BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded(index);

                    // Check for replacement of AI
                    if (buildingInfo != null && buildingInfo.name.Contains("Barracks"))
                    {
                        // custom assets get the farm workers barracks info
                        if(buildingInfo.m_isCustomContent)
                        {
                            buildingInfo.m_class = industriesHousingBuildingInfo.m_class;
                        }
                        AiReplacementHelper.ApplyNewAIToBuilding(buildingInfo, "Barracks");
                    }

                    // Check for updating capacity - Existing CIs will be updated on-load, this will set the data used for placing new homes
                    if (this.loadedLevel == LOADED_LEVEL_GAME && buildingInfo != null && buildingInfo.m_buildingAI is BarracksAI barracksAI) 
                    {
                        barracksAI.updateCapacity(capcityModifier);  
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }
        
        private static IEnumerator ActionWrapper(Action a) {
            a();
            yield break;
        }

        private static void AddQueuedActionsToLoadingQueue() {
            LoadingManager instance = Singleton<LoadingManager>.instance;
            object obj = typeof(LoadingManager).GetFieldByName("m_loadingLock").GetValue(instance);

            while (!Monitor.TryEnter(obj, SimulationManager.SYNCHRONIZE_TIMEOUT)) {
            }
            try {
                FieldInfo fieldByName = typeof(LoadingManager).GetFieldByName("m_mainThreadQueue");
                Queue<IEnumerator> queue1 = (Queue<IEnumerator>) fieldByName.GetValue(instance);
                if (queue1 == null) {
                    return;
                }
                Queue<IEnumerator> queue2 = new Queue<IEnumerator>(queue1.Count + 1);
                queue2.Enqueue(queue1.Dequeue());
                while (!Monitor.TryEnter(QUEUE_LOCK, SimulationManager.SYNCHRONIZE_TIMEOUT));
                try {
                    while (ACTION_QUEUE.Count > 0) {
                        queue2.Enqueue(ACTION_QUEUE.Dequeue());
                    }
                } finally {
                    Monitor.Exit(QUEUE_LOCK);
                }
                while (queue1.Count > 0) {
                    queue2.Enqueue(queue1.Dequeue());
                }
                fieldByName.SetValue(instance, queue2);
            } finally {
                Monitor.Exit(obj);
            }
        }
    }
}