using ICities;
using CitiesHarmony.API;
using CampusIndustriesHousingMod.Utils;
using System;
using CampusIndustriesHousingMod.Managers;
using ColossalFramework;
using CampusIndustriesHousingMod.AI;

namespace CampusIndustriesHousingMod
{
    public class Mod : LoadingExtensionBase, IUserMod, ISerializableData  
    {
        private readonly OptionsManager optionsManager = new();

        public new IManagers managers { get; }

        private static Mod instance;
        string IUserMod.Name => "Campus Industries Housing Mod";

        string IUserMod.Description => "Turn the Dorms and Barracks to actual living spaces apart from their other functions";
        
        public void OnEnabled() 
        {
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
            AtlasUtils.CreateAtlas();
        }

        public void OnDisabled() 
        {
            if (HarmonyHelper.IsHarmonyInstalled) Patcher.UnpatchAll();
        }

        public static Mod GetInstance() 
        {
            return instance;
        }

        public OptionsManager GetOptionsManager() 
        {
            return optionsManager;
        }

        public void OnSettingsUI(UIHelperBase helper) 
        {
            optionsManager.Initialize(helper);
            optionsManager.LoadOptions();
        }

        public override void OnCreated(ILoading loading) 
        {
            Logger.LogInfo(Logger.LOG_BASE, "CampusIndustriesHousingMod Created");
            instance = this;
            try
            {
                HousingManager.Init();
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                HousingManager.Deinit();
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            switch (mode)
            {
                case LoadMode.LoadGame:
                case LoadMode.NewGame:
                case LoadMode.LoadScenario:
                case LoadMode.NewGameFromScenario:
                    break;

                default:
                    return;
            }

            var buildings = Singleton<BuildingManager>.instance.m_buildings;

            for (ushort buildingId = 0; buildingId < buildings.m_size; buildingId++)
            {
                var building = buildings.m_buffer[buildingId];
                if ((building.m_flags & Building.Flags.Created) != 0)
                {
                    if (HousingManager.BuildingRecordExist(buildingId) && building.Info.GetAI() is not BarracksAI && building.Info.GetAI() is not DormsAI)
                    {
                        HousingManager.RemoveBuildingRecord(buildingId);
                    }
                }
            }
        }

        public byte[] LoadData(string id) 
        {
            Logger.LogInfo(Logger.LOG_SERIALIZATION, "Load Data: {0}", id);
            return null;
        }

        public void SaveData(string id, byte[] data) 
        {
            Logger.LogInfo(Logger.LOG_SERIALIZATION, "Save Data: {0} -- {1}", id, data);
        }

        public string[] EnumerateData()
	    {
		    return null;
	    }

        public void EraseData(string id)
	    {
	    }

	    public bool LoadGame(string saveName)
	    {
		    return false;
	    }

	    public bool SaveGame(string saveName)
	    {
		    return false;
	    }
    }
}
