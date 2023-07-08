using ICities;
using CitiesHarmony.API;
using UnityEngine;
using CampusIndustriesHousingMod.Utils;
using CampusIndustriesHousingMod.UI;

namespace CampusIndustriesHousingMod
{
    public class CampusIndustriesHousingMod : LoadingExtensionBase, IUserMod, ISerializableData  
    {
        private const bool LOG_BASE = true;

        private GameObject campusIndustriesHousingInitializerObj;
        private CampusIndustriesHousingInitializer campusIndustriesHousingInitializer;
        private OptionsManager optionsManager = new OptionsManager();
        private static GameObject m_goPanel;
        public static HousingUIPanel Panel { get { return m_goPanel?.GetComponent<HousingUIPanel>(); } }

        public new IManagers managers { get; }

        private static CampusIndustriesHousingMod instance;
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

        public static CampusIndustriesHousingMod getInstance() 
        {
            return instance;
        }

        public CampusIndustriesHousingInitializer getCampusIndustriesHousingInitializer()
	    {
		    return campusIndustriesHousingInitializer;
	    }

        public OptionsManager getOptionsManager() 
        {
            return this.optionsManager;
        }

        public void OnSettingsUI(UIHelperBase helper) 
        {
            this.optionsManager.initialize(helper);
            this.optionsManager.loadOptions();
        }

        public override void OnCreated(ILoading loading) 
        {
            Logger.logInfo(LOG_BASE, "CampusIndustriesHousingMod Created");
            instance = this;
            base.OnCreated(loading);
            if (!(this.campusIndustriesHousingInitializerObj != null)) 
            {
                this.campusIndustriesHousingInitializerObj = new GameObject("CampusIndustriesHousing");
                this.campusIndustriesHousingInitializer = this.campusIndustriesHousingInitializerObj.AddComponent<CampusIndustriesHousingInitializer>();
            }
        }

        public override void OnLevelUnloading()
	    {
		    base.OnLevelUnloading();
		    campusIndustriesHousingInitializer?.OnLevelUnloading();
            if (m_goPanel != null)
            {
                Object.Destroy(m_goPanel);
                m_goPanel = null;
            }
	    }

        public override void OnLevelLoaded(LoadMode mode) 
        {
            Logger.logInfo(true, "CampusIndustriesHousingMod Level Loaded: {0}", mode);
		    base.OnLevelLoaded(mode);
		    switch (mode)
		    {
		        case LoadMode.NewGame:
		        case LoadMode.LoadGame:
                case LoadMode.NewGameFromScenario:
                    campusIndustriesHousingInitializer?.OnLevelWasLoaded(6);
                    m_goPanel = new GameObject("HousingUIGameObject");
                    m_goPanel.AddComponent<HousingUIPanel>();
			    break;
		        case LoadMode.NewAsset:
		        case LoadMode.LoadAsset:
			        campusIndustriesHousingInitializer?.OnLevelWasLoaded(19);
			    break;
		    }
        }

        public override void OnReleased() 
        {
            base.OnReleased();
            if (!HarmonyHelper.IsHarmonyInstalled)
            {
                return;
            }
            if (this.campusIndustriesHousingInitializerObj != null) 
            {
                Object.Destroy(this.campusIndustriesHousingInitializerObj);
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
