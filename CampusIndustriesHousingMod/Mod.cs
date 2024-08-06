using ICities;
using CitiesHarmony.API;
using CampusIndustriesHousingMod.Utils;
using System;
using CampusIndustriesHousingMod.Managers;

namespace CampusIndustriesHousingMod
{
    public class Mod : LoadingExtensionBase, IUserMod, ISerializableData  
    {
        private OptionsManager optionsManager = new();

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

        public static Mod getInstance() 
        {
            return instance;
        }

        public OptionsManager getOptionsManager() 
        {
            return optionsManager;
        }

        public void OnSettingsUI(UIHelperBase helper) 
        {
            optionsManager.initialize(helper);
            optionsManager.loadOptions();
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

        public byte[] LoadData(string id) 
        {
            Logger.LogInfo(Logger.LOG_OPTIONS, "Load Data: {0}", id);
            return null;
        }

        public void SaveData(string id, byte[] data) 
        {
            Logger.LogInfo(Logger.LOG_OPTIONS, "Save Data: {0} -- {1}", id, data);
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
