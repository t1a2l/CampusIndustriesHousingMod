using CampusIndustriesHousingMod.AI;
using CampusIndustriesHousingMod.Utils;
using CampusIndustriesHousingMod.Managers;
using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Linq;
using UnityEngine;


namespace CampusIndustriesHousingMod.UI
{
    public static class HousingUIPanel
    {
        public static UIPanel m_uiMainPanel;

        private static CityServiceWorldInfoPanel m_cityServiceWorldInfoPanel;
        
        private static UILabel m_settingsHeader;
        private static UILabel m_settingsStatus;
        private static UICheckBox m_settingsCheckBox;

        private static UIPanel ApartmentNumberPanel;

        private static UIButton ApplyBuildingSettingsBtn;
        private static UIButton ClearBuildingSettingsBtn;
        private static UIButton ReturnToDefaultBtn;

        private static UIButton ApplyPrefabSettingsBtn;
        private static UIButton ApplyGlobalSettingsBtn;

        private static UIButton SetPrefabSettingsBtn;
        private static UIButton SetGlobalSettingsBtn;

        private static UIButton UnlockSettingsBtn;

        private static readonly float DEFAULT_HEIGHT = 18F;

        public static void Init()
        {
            CreateUI();
        }

        private static void CreateUI()
        {
            m_cityServiceWorldInfoPanel = UIView.library.Get<CityServiceWorldInfoPanel>(typeof(CityServiceWorldInfoPanel).Name);
            UIComponent wrapper = m_cityServiceWorldInfoPanel?.Find("Wrapper");
            UIComponent mainSectionPanel = wrapper?.Find("MainSectionPanel");
            UIComponent mainBottom = mainSectionPanel?.Find("MainBottom");
            UIComponent buttonPanels = mainBottom?.Find("ButtonPanels");
            UIComponent m_parkButtons = buttonPanels?.Find("ParkButtons");
            if(m_parkButtons != null)
            {
                m_uiMainPanel = m_cityServiceWorldInfoPanel.component.AddUIComponent<UIPanel>();
                m_uiMainPanel.name = "HousingUIPanel";
                m_uiMainPanel.backgroundSprite = "SubcategoriesPanel";
                m_uiMainPanel.opacity = 0.90f;
                m_uiMainPanel.isVisible = HousingConfig.Config.ShowPanel;
                m_uiMainPanel.relativePosition = new Vector3(m_uiMainPanel.parent.width + 1f, 40f);
                m_uiMainPanel.height = 370f;
                m_uiMainPanel.width = 510f;

                m_settingsCheckBox = UiUtils.CreateCheckBox(m_parkButtons, "SettingsCheckBox", "settings", HousingConfig.Config.ShowPanel);
                m_settingsCheckBox.width = 110f;
                m_settingsCheckBox.label.textColor = new Color32(185, 221, 254, 255);
                m_settingsCheckBox.label.textScale = 0.8125f;
                m_settingsCheckBox.tooltip = "Indicators will show how well serviced the building is and what problems might prevent the building from leveling up.";
                m_settingsCheckBox.AlignTo(m_cityServiceWorldInfoPanel.component, UIAlignAnchor.TopLeft);
                m_settingsCheckBox.relativePosition = new Vector3(m_uiMainPanel.width - m_settingsCheckBox.width, 6f);
                m_settingsCheckBox.eventCheckChanged += (component, value) =>
                {
                    m_uiMainPanel.isVisible = value;
                    m_uiMainPanel.height = 370f;
                    HousingConfig.Config.ShowPanel = value;
                    if(!value)
                    {
                        ApplyBuildingSettingsBtn.Disable();
                        ClearBuildingSettingsBtn.Disable();
                        ReturnToDefaultBtn.Disable();
                        ApplyPrefabSettingsBtn.Disable();
                        ApplyGlobalSettingsBtn.Disable();
                        SetPrefabSettingsBtn.Disable();
                        SetGlobalSettingsBtn.Disable();
                        UnlockSettingsBtn.Show();
                    }
                    HousingConfig.Config.Serialize();
                };
                m_parkButtons.AttachUIComponent(m_settingsCheckBox.gameObject);

                m_settingsHeader = UiUtils.CreateLabel(m_uiMainPanel, "SettingsPanelHeader", "Settings", "");
                m_settingsHeader.font = UiUtils.GetUIFont("OpenSans-Regular");
                m_settingsHeader.textAlignment = UIHorizontalAlignment.Center;
                m_settingsHeader.relativePosition = new Vector3(10f, 60f + 0 * (DEFAULT_HEIGHT * 0.8f + 2f));

                m_settingsStatus = UiUtils.CreateLabel(m_uiMainPanel, "SettingsStatus", "", "");
                m_settingsStatus.font = UiUtils.GetUIFont("OpenSans-Regular");
                m_settingsStatus.textAlignment = UIHorizontalAlignment.Center;
                m_settingsStatus.textColor = new Color32(215, 51, 58, 255);
                m_settingsStatus.relativePosition = new Vector3(100f, 20f);
                m_settingsStatus.textScale = 1.2f;

                ApartmentNumberPanel = UiUtils.UIServiceBar(m_uiMainPanel, "ApartmentNumber", "", "Number of apartments: ", "number of apartments");
                ApartmentNumberPanel.relativePosition = new Vector3(10f, 60f + 2 * (DEFAULT_HEIGHT * 0.8f + 2f));

                ApplyBuildingSettingsBtn = UiUtils.AddButton(m_uiMainPanel, 260f, 60f + 2 * (DEFAULT_HEIGHT * 0.8f + 2f), "ApplyBuildingSettings", "Apply building settings", "First priority - will override prefab and global settings create a record for this building");
                ApplyBuildingSettingsBtn.eventClicked += ApplyBuildingSettings;

                ClearBuildingSettingsBtn = UiUtils.AddButton(m_uiMainPanel, 260f, 60f + 5 * (DEFAULT_HEIGHT * 0.8f + 2f), "ClearBuildingSettings", "Clear building settings", "Clear tis building record - will get the values from prefab or global settings");
                ClearBuildingSettingsBtn.eventClicked += ClearBuildingSettings;

                ReturnToDefaultBtn = UiUtils.AddButton(m_uiMainPanel, 260f, 60f + 8 * (DEFAULT_HEIGHT * 0.8f + 2f), "ReturnToDefault", "Back to default", "Will not delete the record just set a default flag on it - you need to clear settings for this building to get the prefab or global settings");
                ReturnToDefaultBtn.eventClicked += ReturnToDefault;

                ApplyPrefabSettingsBtn = UiUtils.AddButton(m_uiMainPanel, 260f, 60f + 11 * (DEFAULT_HEIGHT * 0.8f + 2f), "ApplyPrefabSettings", "Apply type settings", "Apply settings for all buildings of the same type as this building - is not cross save!");
                ApplyPrefabSettingsBtn.eventClicked += ApplyPrefabSettings;

                ApplyGlobalSettingsBtn = UiUtils.AddButton(m_uiMainPanel, 260f, 60f + 14 * (DEFAULT_HEIGHT * 0.8f + 2f), "ApplyGlobalSettings", "Apply global settings", "Apply settings for all buildings of the same type as this building - is cross save!");
                ApplyGlobalSettingsBtn.eventClicked += ApplyGlobalSettings;
                
                SetPrefabSettingsBtn = UiUtils.AddButton(m_uiMainPanel, 10f, 60f + 5 * (DEFAULT_HEIGHT * 0.8f + 2f), "SetPrefabSettings", "Set new type", "This will update all building records of this type to the current number of apartments in this save");
                SetPrefabSettingsBtn.eventClicked += SetPrefabSettings;

                SetGlobalSettingsBtn = UiUtils.AddButton(m_uiMainPanel, 10f, 60f + 8 * (DEFAULT_HEIGHT * 0.8f + 2f), "SetGlobalSettings", "Set new global", "This will update all building records of this type to the current number of apartments across all saves");
                SetGlobalSettingsBtn.eventClicked += SetGlobalSettings;

                UnlockSettingsBtn = UiUtils.AddButton(m_uiMainPanel, 260f, 55f + 0 * (DEFAULT_HEIGHT * 0.8f + 2f), "UnlockSettingsBtn", "Unlock Settings", "");
                UnlockSettingsBtn.eventClicked += UnlockSettings;

                ApplyBuildingSettingsBtn.Disable();
                ClearBuildingSettingsBtn.Disable();
                ReturnToDefaultBtn.Disable();
                ApplyPrefabSettingsBtn.Disable();
                ApplyGlobalSettingsBtn.Disable();
                SetPrefabSettingsBtn.Disable();
                SetGlobalSettingsBtn.Disable();
            }
        }

        public static void UnlockSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ApplyBuildingSettingsBtn.Enable();
            ClearBuildingSettingsBtn.Enable();
            ReturnToDefaultBtn.Enable();
            ApplyPrefabSettingsBtn.Enable();
            ApplyGlobalSettingsBtn.Enable();
            SetPrefabSettingsBtn.Enable();
            SetGlobalSettingsBtn.Enable();

            UnlockSettingsBtn.Hide();
        }

        public static void RefreshData()
        {
            ushort buildingID = WorldInfoPanel.GetCurrentInstanceID().Building;
            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID];
            PrefabAI buildingAI = building.Info.GetAI();
            if (buildingAI is not BarracksAI && buildingAI is not DormsAI)
			{
                m_settingsCheckBox.Hide();
                m_uiMainPanel.Hide();
			}
            else
			{
                string buildingAIstr = "";
                int numOfApartments = 0;
                if(buildingAI is BarracksAI)
                {
                    buildingAIstr = "BarracksAI";
                }
                else if(buildingAI is DormsAI dormsAI)
                {
                    buildingAIstr = "DormsAI";
                }

                var m_apartmentsNumTextfield = ApartmentNumberPanel.Find<UITextField>("ApartmentNumberTextField");               

                var res = HousingManager.BuildingRecords.TryGetValue(buildingID, out HousingManager.BuildingRecord buildingRecord);
                if(res)
                {
                    if(buildingRecord.DefaultValues)
                    {
                        m_settingsStatus.text = "This Building is using default settings";
                    } 
                    else
                    {
                        m_settingsStatus.text = "This Building is using his own settings";
                    }

                    m_apartmentsNumTextfield.text = buildingRecord.NumOfApartments.ToString();
                    numOfApartments = buildingRecord.NumOfApartments;
                } 
                else
                {
                    var prefab_index = HousingManager.PrefabRecords.FindIndex(item => item.Name == building.Info.name && item.BuildingAI == buildingAIstr);
                    if(prefab_index != -1)
                    {
                        m_settingsStatus.text = "This Building is using prefab settings";
                        var prefabRecord = HousingManager.PrefabRecords[prefab_index];
                        m_apartmentsNumTextfield.text = prefabRecord.NumOfApartments.ToString();
                        numOfApartments = prefabRecord.NumOfApartments;
                    }
                    else
                    {
                        var global_index = HousingConfig.Config.HousingSettings.FindIndex(item => item.Name == building.Info.name && item.BuildingAI == buildingAIstr);
                        if(global_index != -1)
                        {
                            m_settingsStatus.text = "This Building is using global settings";
                            var saved_config = HousingConfig.Config.HousingSettings[global_index];
                            m_apartmentsNumTextfield.text = saved_config.NumOfApartments.ToString();
							numOfApartments = saved_config.NumOfApartments;
                        }
                        else
                        {
                            m_settingsStatus.text = "This Building is using default settings";
                            if(buildingAIstr == "BarracksAI")
                            {
                                BarracksAI barracksAI = buildingAI as BarracksAI;
                                barracksAI = HousingManager.DefaultBarracksValues(barracksAI);
                                m_apartmentsNumTextfield.text = barracksAI.numApartments.ToString();
                                numOfApartments = barracksAI.numApartments;
                            }
                            else if(buildingAIstr == "DormsAI")
                            {
                                DormsAI dormsAI = buildingAI as DormsAI;
                                dormsAI = HousingManager.DefaultDormsValues(dormsAI);
                                m_apartmentsNumTextfield.text = dormsAI.numApartments.ToString();
                                numOfApartments = dormsAI.numApartments;
                            }
                        }
                    }
                }
                UpdateHouse(buildingID, ref building, numOfApartments);
                CreateOrEnsure(false, buildingID, ref building, numOfApartments, 0, 0);
                m_settingsCheckBox.Show();
                if(m_settingsCheckBox.isChecked)
                {
                    m_uiMainPanel.height = 370f;
                    m_uiMainPanel.Show();
                }
			}
        }

        public static void ApplyBuildingSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ApplySettings(false, true, false, false);
        }

        public static void ClearBuildingSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ushort buildingID = WorldInfoPanel.GetCurrentInstanceID().Building;
            HousingManager.RemoveBuildingRecord(buildingID);
            RefreshData();
        }
        
        public static void ReturnToDefault(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ApplySettings(true, true, false, false);
        }

        public static void ApplyPrefabSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ApplySettings(false, false, true, false);
        }

        public static void ApplyGlobalSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ApplySettings(false, false, false, true);
        }

        private static void UpdateHouse(ushort buildingID, ref Building data, int numOfApartments)
	    {
            // Validate the capacity and adjust accordingly - but don't create new units, that will be done by EnsureCitizenUnits
            float capacityModifier = Mod.getInstance().getOptionsManager().getDormsCapacityModifier();
            if(data.Info.GetAI() is BarracksAI barracksAI)
            {
                barracksAI.updateCapacity(capacityModifier);
                barracksAI.validateCapacity(buildingID, ref data, false);
                barracksAI.numApartments = numOfApartments;
            }
            else if(data.Info.GetAI() is DormsAI dormsAI)
            {
                dormsAI.updateCapacity(capacityModifier);
                dormsAI.validateCapacity(buildingID, ref data, false);
                dormsAI.numApartments = numOfApartments;
            }
	    }

        private static void EnsureCitizenUnits(ushort buildingID, ref Building data, int homeCount = 0, int workCount = 0, int visitCount = 0, int studentCount = 0, int hotelCount = 0)
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
	        if (instance.CreateUnits(out uint firstUnit, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, homeCount, workCount, visitCount, 0, studentCount, hotelCount))
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

        private static void ApplySettings(bool setDefault, bool isBuilding, bool isPrefab, bool isGlobal)
        {
            ushort buildingID = WorldInfoPanel.GetCurrentInstanceID().Building;
            BuildingInfo buildingInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info;

            var buildingAI = buildingInfo.GetAI();

            var BuildingAIstr = "";

            if(buildingAI is BarracksAI)
            {
                BuildingAIstr = "BarracksAI";
            }
            else if(buildingAI is DormsAI)
            {
                BuildingAIstr = "DormsAI";
            }

            var m_apartmentsNumTextfield = ApartmentNumberPanel.Find<UITextField>("ApartmentNumberTextField");

            // if set to default
            if(setDefault)
            {
                if(buildingAI is BarracksAI barracksAI)
                {
                    barracksAI = HousingManager.DefaultBarracksValues(barracksAI);
                    m_apartmentsNumTextfield.text = barracksAI.numApartments.ToString();
                }
                else if(buildingAI is DormsAI dormsAI)
                {
                    dormsAI = HousingManager.DefaultDormsValues(dormsAI);
                    m_apartmentsNumTextfield.text = dormsAI.numApartments.ToString();
                }
            }
           
            if(isBuilding)
            {
                var buildingRecord = HousingManager.GetBuildingRecord(buildingID);

                buildingRecord.NumOfApartments = int.Parse(m_apartmentsNumTextfield.text);
                buildingRecord.BuildingAI = BuildingAIstr;
                buildingRecord.DefaultValues = setDefault;

                HousingManager.SetBuildingRecord(buildingID, buildingRecord);
			}
			else if(isPrefab)
			{
                var prefabRecord = HousingManager.GetPrefab(buildingInfo.name, BuildingAIstr);

                prefabRecord.NumOfApartments = int.Parse(m_apartmentsNumTextfield.text);
                prefabRecord.BuildingAI = BuildingAIstr;

                HousingManager.SetPrefab(prefabRecord);
            }
            else if(isGlobal)
            {
                var housing = HousingConfig.Config.GetGlobalSettings(buildingInfo.name, BuildingAIstr);

                housing.Name = buildingInfo.name;
                housing.BuildingAI = BuildingAIstr;
                housing.NumOfApartments = int.Parse(m_apartmentsNumTextfield.text);

                HousingConfig.Config.SetGlobalSettings(housing);
            }

            RefreshData();
        }

        public static void SetPrefabSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ConfirmPanel.ShowModal("Set Prefab Settings", "This will update all building records of this type to the current number of apartments in this save!", (comp, ret) =>
            {
                if (ret != 1)
                    return;
                SetPrefabGlobalSettings(false);
            });
        }

        public static void SetGlobalSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ConfirmPanel.ShowModal("Set Global Settings", "This will update all building records of this type to the current number of apartments across all saves!", (comp, ret) =>
            {
                if (ret != 1)
                    return;
                SetPrefabGlobalSettings(true);
            });
        }

        private static void SetPrefabGlobalSettings(bool isGlobal)
        {
            ushort buildingID = WorldInfoPanel.GetCurrentInstanceID().Building;
            BuildingInfo buildingInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info;

            var buildingAI = buildingInfo.GetAI();

            var BuildingAIstr = "";

            if(buildingAI is BarracksAI)
            {
                BuildingAIstr = "BarracksAI";
            }
            else if(buildingAI is DormsAI)
            {
                BuildingAIstr = "DormsAI";
            }
           
            var buildingsList = HousingManager.BuildingRecords.Where(item =>
		    {
                BuildingInfo Info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[item.Key].Info;
                return Info.name == buildingInfo.name && item.Value.BuildingAI == BuildingAIstr;
            }).ToList();

            foreach( var item in buildingsList )
            {
                HousingManager.BuildingRecords.Remove(item.Key);
            }

            if(isGlobal)
            {
                var buildingsPrefabList = HousingManager.PrefabRecords.Where(item =>
			    {
                    return item.Name == buildingInfo.name && item.BuildingAI == BuildingAIstr;
                }).ToList();

                foreach( var item in buildingsPrefabList )
                {
                    HousingManager.PrefabRecords.Remove(item);
                }
            }

            RefreshData();
        }

        public static void CreateOrEnsure(bool is_new, ushort buildingID, ref Building data, int numOfApartments, int workCount, int studentCount)
        {
            if(is_new)
            {
                Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, numOfApartments, workCount, 0, 0, 0);
            }
            else
            {
                EnsureCitizenUnits(buildingID, ref data, numOfApartments, workCount);
            }
        }


        public static void LoadSettings(ushort buildingID, ref Building data, bool is_new)
        {
            BuildingInfo buildingInfo = data.Info;
            PrefabAI buildingAI = buildingInfo.GetAI();
            string buildingAIstr = "";
            int numOfApartments = 0;

            if (buildingAI is BarracksAI)
            {
                buildingAIstr = "BarracksAI";
            }
            else if (buildingAI is DormsAI dormsAI)
            {
                buildingAIstr = "DormsAI";
            }

            var res = HousingManager.BuildingRecords.TryGetValue(buildingID, out HousingManager.BuildingRecord buildingRecord);
            if (res)
            {
                numOfApartments = buildingRecord.NumOfApartments;
            }
            else
            {
                var prefab_index = HousingManager.PrefabRecords.FindIndex(item => item.Name == buildingInfo.name && item.BuildingAI == buildingAIstr);
                if (prefab_index != -1)
                {
                    var prefabRecord = HousingManager.PrefabRecords[prefab_index];
                    numOfApartments = prefabRecord.NumOfApartments;
                }
                else
                {
                    var global_index = HousingConfig.Config.HousingSettings.FindIndex(item => item.Name == buildingInfo.name && item.BuildingAI == buildingAIstr);
                    if (global_index != -1)
                    {
                            
                        var saved_config = HousingConfig.Config.HousingSettings[global_index];
                        numOfApartments = saved_config.NumOfApartments;
                    }
                    else
                    {
                        if (buildingAIstr == "BarracksAI")
                        {
                            BarracksAI barracksAI = buildingAI as BarracksAI;
                            barracksAI = HousingManager.DefaultBarracksValues(barracksAI);
                            numOfApartments = barracksAI.numApartments;
                        }
                        else if (buildingAIstr == "DormsAI")
                        {
                            DormsAI dormsAI = buildingAI as DormsAI;
                            dormsAI = HousingManager.DefaultDormsValues(dormsAI);
                            numOfApartments = dormsAI.numApartments;
                        }
                    }
                }
            }

            UpdateHouse(buildingID, ref data, numOfApartments);
            CreateOrEnsure(is_new, buildingID, ref data, numOfApartments, 0, 0);
        }
    }

}
