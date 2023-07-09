using ICities;
using CitiesHarmony.API;
using UnityEngine;
using CampusIndustriesHousingMod.Utils;
using System;

namespace CampusIndustriesHousingMod
{
    public class Mod : LoadingExtensionBase, IUserMod, ISerializableData  
    {
        private const bool LOG_BASE = true;

        private GameObject campusIndustriesHousingInitializerObj;
        private CampusIndustriesHousingInitializer campusIndustriesHousingInitializer;
        private OptionsManager optionsManager = new OptionsManager();

        public new IManagers managers { get; }

        private static Mod instance;
        string IUserMod.Name => "Campus Industries Housing Mod";

        string IUserMod.Description => "Turn the Dorms and Barracks to actual living spaces apart from their other functions";
        
        public void OnEnabled() 
        {
             HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled() 
        {
            if (HarmonyHelper.IsHarmonyInstalled) Patcher.UnpatchAll();
        }

        public static Mod getInstance() 
        {
            return instance;
        }

        public CampusIndustriesHousingInitializer getCampusIndustriesHousingInitializer()
	    {
		    return campusIndustriesHousingInitializer;
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
            Logger.logInfo(LOG_BASE, "CampusIndustriesHousingMod Created");
            instance = this;
            base.OnCreated(loading);
            if (campusIndustriesHousingInitializerObj == null) 
            {
                campusIndustriesHousingInitializerObj = new GameObject("CampusIndustriesHousing");
                campusIndustriesHousingInitializer = campusIndustriesHousingInitializerObj.AddComponent<CampusIndustriesHousingInitializer>();
            }
            try
            {
                HousingManager.Init();
            }
            catch (Exception e)
            {
                HousingManager.Deinit();
            }

        }

        public override void OnLevelUnloading()
	    {
		    base.OnLevelUnloading();
		    campusIndustriesHousingInitializer?.OnLevelUnloading();
	    }

        public override void OnLevelLoaded(LoadMode mode) 
        {
            Logger.logInfo(true, "CampusIndustriesHousingMod Level Loaded: {0}", mode);
		    base.OnLevelLoaded(mode);
            if(mode == LoadMode.NewGame || mode == LoadMode.LoadGame || mode == LoadMode.NewGameFromScenario)
            {
                campusIndustriesHousingInitializer?.OnLevelWasLoaded(6);
            }
            if(mode == LoadMode.NewAsset || mode == LoadMode.LoadAsset)
            {
                campusIndustriesHousingInitializer?.OnLevelWasLoaded(19);
            }
        }

        public override void OnReleased() 
        {
            base.OnReleased();
            if (!HarmonyHelper.IsHarmonyInstalled)
            {
                return;
            }
            if (campusIndustriesHousingInitializerObj != null) 
            {
                UnityEngine.Object.Destroy(campusIndustriesHousingInitializerObj);
            }
        }

        public byte[] LoadData(string id) 
        {
            Logger.logInfo(Logger.LOG_OPTIONS, "Load Data: {0}", id);
            return null;
        }

        public void SaveData(string id, byte[] data) 
        {
            Logger.logInfo(Logger.LOG_OPTIONS, "Save Data: {0} -- {1}", id, data);
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
