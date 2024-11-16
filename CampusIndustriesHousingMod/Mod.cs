using ICities;
using CitiesHarmony.API;
using CampusIndustriesHousingMod.Utils;
using System;
using CampusIndustriesHousingMod.Managers;
using ColossalFramework;
using CampusIndustriesHousingMod.AI;
using UnityEngine;

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
                ref var building = ref buildings.m_buffer[buildingId];
                if ((building.m_flags & Building.Flags.Created) != 0)
                {
                    if (building.Info.GetAI() is DormsAI || building.Info.GetAI() is BarracksAI)
                    {
                        HousingManager.BuildingRecord buildingRecord;
                        if (HousingManager.BuildingRecordExist(buildingId))
                        {
                            buildingRecord = HousingManager.GetBuildingRecord(buildingId);
                        }
                        else
                        {
                            buildingRecord = HousingManager.CreateBuildingRecord(buildingId);
                        }
                        if (building.Info.GetAI() is DormsAI dormsAI)
                        {
                            dormsAI.ValidateCapacity(buildingId, ref building, false);
                        }
                        else if (building.Info.GetAI() is BarracksAI barracksAI)
                        {
                            barracksAI.ValidateCapacity(buildingId, ref building, false);
                        }
                        EnsureCitizenUnits(buildingId, ref building, buildingRecord.NumOfApartments);
                    }
                    else
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

        protected void EnsureCitizenUnits(ushort buildingID, ref Building data, int homeCount = 0, int workCount = 0, int visitCount = 0, int studentCount = 0, int hotelCount = 0)
        {
            if ((data.m_flags & (Building.Flags.Abandoned | Building.Flags.Collapsed)) != 0)
            {
                return;
            }

            Citizen.Wealth wealthLevel = Citizen.GetWealthLevel((ItemClass.Level)data.m_level);
            CitizenManager instance = Singleton<CitizenManager>.instance;
            uint num = 0u;
            uint num2 = data.m_citizenUnits;
            int num3 = 0;
            while (num2 != 0)
            {
                CitizenUnit.Flags flags = instance.m_units.m_buffer[num2].m_flags;
                if ((flags & CitizenUnit.Flags.Home) != 0)
                {
                    instance.m_units.m_buffer[num2].SetWealthLevel(wealthLevel);
                    homeCount--;
                }

                if ((flags & CitizenUnit.Flags.Work) != 0)
                {
                    workCount -= 5;
                }

                if ((flags & CitizenUnit.Flags.Visit) != 0)
                {
                    visitCount -= 5;
                }

                if ((flags & CitizenUnit.Flags.Student) != 0)
                {
                    studentCount -= 5;
                }

                num = num2;
                num2 = instance.m_units.m_buffer[num2].m_nextUnit;
                if (++num3 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }

            homeCount = Mathf.Max(0, homeCount);
            workCount = Mathf.Max(0, workCount);
            visitCount = Mathf.Max(0, visitCount);
            studentCount = Mathf.Max(0, studentCount);
            hotelCount = Mathf.Max(0, hotelCount);
            if (homeCount == 0 && workCount == 0 && visitCount == 0 && studentCount == 0 && hotelCount == 0)
            {
                return;
            }

            uint firstUnit = 0u;
            if (instance.CreateUnits(out firstUnit, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, homeCount, workCount, visitCount, 0, studentCount, hotelCount))
            {
                if (num != 0)
                {
                    instance.m_units.m_buffer[num].m_nextUnit = firstUnit;
                }
                else
                {
                    data.m_citizenUnits = firstUnit;
                }
            }
        }

    }
}
