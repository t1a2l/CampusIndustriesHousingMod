using CampusIndustriesHousingMod.AI;
using CampusIndustriesHousingMod.Utils;
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
        private static UIPanel WorkPlaceCount0Panel;
        private static UIPanel WorkPlaceCount1Panel;
        private static UIPanel WorkPlaceCount2Panel;
        private static UIPanel WorkPlaceCount3Panel;

        private static UIButton SaveBuildingSettingsBtn;
        private static UIButton ClearBuildingSettingsBtn;
        private static UIButton SaveDefaultSettingsBtn;
        private static UIButton SavePrefabSettingsBtn;
        private static UIButton SaveGlobalSettingsBtn;

        private static UIButton ApplyPrefabSettingsBtn;
        private static UIButton ApplyGlobalSettingsBtn; 

        private static float DEFAULT_HEIGHT = 18F;

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
                m_uiMainPanel.height = m_uiMainPanel.parent.height - 7f;
                m_uiMainPanel.isVisible = HousingConfig.Config.ShowPanel;
                m_uiMainPanel.relativePosition = new Vector3(m_uiMainPanel.parent.width + 1f, m_uiMainPanel.parent.position.y + 40f);
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
                    m_uiMainPanel.height = m_uiMainPanel.parent.height - 7f;
                    HousingConfig.Config.ShowPanel = value;
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

                WorkPlaceCount0Panel = UiUtils.UIServiceBar(m_uiMainPanel, "WorkPlaceCount0", "", "Uneducated Workers: ", "number of uneducated workers");
                WorkPlaceCount0Panel.relativePosition = new Vector3(10f, 60f + 4 * (DEFAULT_HEIGHT * 0.8f + 2f));

                WorkPlaceCount1Panel = UiUtils.UIServiceBar(m_uiMainPanel, "WorkPlaceCount1", "", "Educated Workers: ", "number of educated workers");
                WorkPlaceCount1Panel.relativePosition = new Vector3(10f, 60f + 6 * (DEFAULT_HEIGHT * 0.8f + 2f));

                WorkPlaceCount2Panel = UiUtils.UIServiceBar(m_uiMainPanel, "WorkPlaceCount2", "", "Well Educated Workers: ", "number of well educated workers");
                WorkPlaceCount2Panel.relativePosition = new Vector3(10f, 60f + 8 * (DEFAULT_HEIGHT * 0.8f + 2f));

                WorkPlaceCount3Panel = UiUtils.UIServiceBar(m_uiMainPanel, "WorkPlaceCount3", "", "Highly Educated Workers: ", "number of highly educated workers");
                WorkPlaceCount3Panel.relativePosition = new Vector3(10f, 60f + 10 * (DEFAULT_HEIGHT * 0.8f + 2f));
                
                SaveBuildingSettingsBtn = UiUtils.AddButton(m_uiMainPanel, 260f, 60f + 2 * (DEFAULT_HEIGHT * 0.8f + 2f), "SaveBuildingSettings", "save building settings", "first priority - will override prefab and global settings create a record for this building");
                SaveBuildingSettingsBtn.eventClicked += SaveBuildingSettings;

                ClearBuildingSettingsBtn = UiUtils.AddButton(m_uiMainPanel, 260f, 60f + 5 * (DEFAULT_HEIGHT * 0.8f + 2f), "ClearBuildingSettings", "clear building settings", "clear tis building record - will get the values from prefab or global settings");
                ClearBuildingSettingsBtn.eventClicked += ClearBuildingSettings;

                SaveDefaultSettingsBtn = UiUtils.AddButton(m_uiMainPanel, 260f, 60f + 8 * (DEFAULT_HEIGHT * 0.8f + 2f), "ReturnToDefault", "back to default", "will not delete the record just set a default flag on it - you need to clear settings for this building to get the prefab or global settings");            
                SaveDefaultSettingsBtn.eventClicked += SaveDefaultSettings;

                SavePrefabSettingsBtn = UiUtils.AddButton(m_uiMainPanel, 260f, 60f + 11 * (DEFAULT_HEIGHT * 0.8f + 2f), "SavePrefabSettings", "save as prefab settings", "save settings for all buildings of the same type as this building - is not cross save!");
                SavePrefabSettingsBtn.eventClicked += SavePrefabSettings;

                SaveGlobalSettingsBtn = UiUtils.AddButton(m_uiMainPanel, 260f, 60f + 14 * (DEFAULT_HEIGHT * 0.8f + 2f), "SaveGlobalSettings", "save as global settings", "save settings for all buildings of the same type as this building - is cross save!");            
                SaveGlobalSettingsBtn.eventClicked += SaveGlobalSettings;
                
                ApplyPrefabSettingsBtn = UiUtils.AddButton(m_uiMainPanel, 10f, 60f + 13 * (DEFAULT_HEIGHT * 0.8f + 2f), "ApplyPrefabSettings", "apply prefab", "ignore all building records of the same type and deletes the records to apply prefab settings");
                ApplyPrefabSettingsBtn.eventClicked += ApplyPrefabSettings;

                ApplyGlobalSettingsBtn = UiUtils.AddButton(m_uiMainPanel, 10f, 60f + 16 * (DEFAULT_HEIGHT * 0.8f + 2f), "ApplyGlobalSettings", "apply global", "ignore all building records and prefabs of the same type and deletes the records and prefabs to apply global settings");            
                ApplyGlobalSettingsBtn.eventClicked += ApplyGlobalSettings;

            }
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
                int WorkPlaceCount0 = 0;
                int WorkPlaceCount1 = 0;
                int WorkPlaceCount2 = 0;
                int WorkPlaceCount3 = 0;
                if(buildingAI is BarracksAI)
                {
                    buildingAIstr = "BarracksAI";
                }
                else if(buildingAI is DormsAI)
                {
                    buildingAIstr = "DormsAI";
                }

                var m_apartmentsNumTextfield = ApartmentNumberPanel.Find<UITextField>("ApartmentNumberTextField");
                var m_workPlaceCount0Textfield = WorkPlaceCount0Panel.Find<UITextField>("WorkPlaceCount0TextField");
                var m_workPlaceCount1Textfield = WorkPlaceCount1Panel.Find<UITextField>("WorkPlaceCount1TextField");
                var m_workPlaceCount2Textfield = WorkPlaceCount2Panel.Find<UITextField>("WorkPlaceCount2TextField");
                var m_workPlaceCount3Textfield = WorkPlaceCount3Panel.Find<UITextField>("WorkPlaceCount3TextField");
                

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
                    m_workPlaceCount0Textfield.text = buildingRecord.WorkPlaceCount0.ToString();
                    m_workPlaceCount1Textfield.text = buildingRecord.WorkPlaceCount1.ToString();
                    m_workPlaceCount2Textfield.text = buildingRecord.WorkPlaceCount2.ToString();
                    m_workPlaceCount3Textfield.text = buildingRecord.WorkPlaceCount3.ToString();
                    numOfApartments = buildingRecord.NumOfApartments;
                    WorkPlaceCount0 = buildingRecord.WorkPlaceCount0;
                    WorkPlaceCount1 = buildingRecord.WorkPlaceCount1;
                    WorkPlaceCount2 = buildingRecord.WorkPlaceCount2;
                    WorkPlaceCount3 = buildingRecord.WorkPlaceCount3;
                } 
                else
                {
                    var prefab_index = HousingManager.PrefabRecords.FindIndex(item => item.Name == building.Info.name && item.BuildingAI == buildingAIstr);
                    if(prefab_index != -1)
                    {
                        m_settingsStatus.text = "This Building is using prefab settings";
                        var prefabRecord = HousingManager.PrefabRecords[prefab_index];
                        m_apartmentsNumTextfield.text = prefabRecord.NumOfApartments.ToString();
                        m_workPlaceCount0Textfield.text = prefabRecord.WorkPlaceCount0.ToString();
                        m_workPlaceCount1Textfield.text = prefabRecord.WorkPlaceCount1.ToString();
                        m_workPlaceCount2Textfield.text = prefabRecord.WorkPlaceCount2.ToString();
                        m_workPlaceCount3Textfield.text = prefabRecord.WorkPlaceCount3.ToString();
                        numOfApartments = prefabRecord.NumOfApartments;
                        WorkPlaceCount0 = prefabRecord.WorkPlaceCount0;
                        WorkPlaceCount1 = prefabRecord.WorkPlaceCount1;
                        WorkPlaceCount2 = prefabRecord.WorkPlaceCount2;
                        WorkPlaceCount3 = prefabRecord.WorkPlaceCount3;
                    }
                    else
                    {
                        var global_index = HousingConfig.Config.HousingSettings.FindIndex(item => item.Name == building.Info.name && item.BuildingAI == buildingAIstr);
                        if(global_index != -1)
                        {
                            m_settingsStatus.text = "This Building is using global settings";
                            var saved_config = HousingConfig.Config.HousingSettings[global_index];
                            m_apartmentsNumTextfield.text = saved_config.NumOfApartments.ToString();
                            m_workPlaceCount0Textfield.text = saved_config.WorkPlaceCount0.ToString();
                            m_workPlaceCount1Textfield.text = saved_config.WorkPlaceCount1.ToString();
                            m_workPlaceCount2Textfield.text = saved_config.WorkPlaceCount2.ToString();
                            m_workPlaceCount3Textfield.text = saved_config.WorkPlaceCount3.ToString();
							numOfApartments = saved_config.NumOfApartments;
                            WorkPlaceCount0 = saved_config.WorkPlaceCount0;
                            WorkPlaceCount1 = saved_config.WorkPlaceCount1;
                            WorkPlaceCount2 = saved_config.WorkPlaceCount2;
                            WorkPlaceCount3 = saved_config.WorkPlaceCount3;
                        }
                        else
                        {
                            m_settingsStatus.text = "This Building is using default settings";
                            if(buildingAIstr == "BarracksAI")
                            {
                                BarracksAI barracksAI = buildingAI as BarracksAI;
                                barracksAI = HousingManager.DefaultBarracksValues(barracksAI);
                                m_apartmentsNumTextfield.text = barracksAI.numApartments.ToString();
                                m_workPlaceCount0Textfield.text = barracksAI.m_workPlaceCount0.ToString();
                                m_workPlaceCount1Textfield.text = barracksAI.m_workPlaceCount1.ToString();
                                m_workPlaceCount2Textfield.text = barracksAI.m_workPlaceCount2.ToString();
                                m_workPlaceCount3Textfield.text = barracksAI.m_workPlaceCount3.ToString();
                                numOfApartments = barracksAI.numApartments;
                                WorkPlaceCount0 = barracksAI.m_workPlaceCount0;
                                WorkPlaceCount1 = barracksAI.m_workPlaceCount1;
                                WorkPlaceCount2 = barracksAI.m_workPlaceCount2;
                                WorkPlaceCount3 = barracksAI.m_workPlaceCount3;
                            }
                            else if(buildingAIstr == "DormsAI")
                            {
                                DormsAI dormsAI = buildingAI as DormsAI;
                                dormsAI = HousingManager.DefaultDormsValues(dormsAI);
                                m_apartmentsNumTextfield.text = dormsAI.numApartments.ToString();
                                m_workPlaceCount0Textfield.text = dormsAI.m_workPlaceCount0.ToString();
                                m_workPlaceCount1Textfield.text = dormsAI.m_workPlaceCount1.ToString();
                                m_workPlaceCount2Textfield.text = dormsAI.m_workPlaceCount2.ToString();
                                m_workPlaceCount3Textfield.text = dormsAI.m_workPlaceCount3.ToString();
                                numOfApartments = dormsAI.numApartments;
                                WorkPlaceCount0 = dormsAI.m_workPlaceCount0;
                                WorkPlaceCount1 = dormsAI.m_workPlaceCount1;
                                WorkPlaceCount2 = dormsAI.m_workPlaceCount2;
                                WorkPlaceCount3 = dormsAI.m_workPlaceCount3;
                            }
                        }
                    }
                }
                UpdateHouse(buildingID, ref building, numOfApartments, WorkPlaceCount0, WorkPlaceCount1, WorkPlaceCount2, WorkPlaceCount3);
                m_uiMainPanel.height = m_uiMainPanel.parent.height - 7f;
                m_settingsCheckBox.Show();
			}
        }

        public static void SaveBuildingSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            SaveSettings(false, true, false, false);
        }

        public static void ClearBuildingSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ushort buildingID = WorldInfoPanel.GetCurrentInstanceID().Building;
            HousingManager.RemoveBuildingRecord(buildingID);
            RefreshData();
        }
        
        public static void SaveDefaultSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            SaveSettings(true, true, false, false);
        }

        public static void SavePrefabSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            SaveSettings(false, false, true, false);
        }

        public static void SaveGlobalSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            SaveSettings(false, false, false, true);
        }

        private static void UpdateHouse(ushort buildingID, ref Building data, int numOfApartments, int WorkPlaceCount0, int WorkPlaceCount1, int WorkPlaceCount2, int WorkPlaceCount3)
	    {
            // Validate the capacity and adjust accordingly - but don't create new units, that will be done by EnsureCitizenUnits
            float capacityModifier = Mod.getInstance().getOptionsManager().getDormsCapacityModifier();
            var NumOfApartments = capacityModifier > 0 ? (int) (numOfApartments * capacityModifier) : numOfApartments;
            if(data.Info.GetAI() is BarracksAI barracksAI)
            {
                barracksAI.updateCapacity(capacityModifier);
                barracksAI.validateCapacity(buildingID, ref data, false);
                barracksAI.numApartments = numOfApartments;
                barracksAI.m_workPlaceCount0 = WorkPlaceCount0;
                barracksAI.m_workPlaceCount1 = WorkPlaceCount1;
                barracksAI.m_workPlaceCount2 = WorkPlaceCount2;
                barracksAI.m_workPlaceCount3 = WorkPlaceCount3;
            }
            else if(data.Info.GetAI() is DormsAI dormsAI)
            {
                dormsAI.updateCapacity(capacityModifier);
                dormsAI.validateCapacity(buildingID, ref data, false);
                dormsAI.numApartments = numOfApartments;
                dormsAI.m_workPlaceCount0 = WorkPlaceCount0;
                dormsAI.m_workPlaceCount1 = WorkPlaceCount1;
                dormsAI.m_workPlaceCount2 = WorkPlaceCount2;
                dormsAI.m_workPlaceCount3 = WorkPlaceCount3;
            }
            int workCount = WorkPlaceCount0 + WorkPlaceCount1 + WorkPlaceCount2 + WorkPlaceCount3;
            EnsureCitizenUnits(buildingID, ref data, NumOfApartments, workCount, 0, 0);
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

        private static void SaveSettings(bool setDefault, bool isBuilding, bool isPrefab, bool isGlobal)
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
            var m_workPlaceCount0Textfield = WorkPlaceCount0Panel.Find<UITextField>("WorkPlaceCount0TextField");
            var m_workPlaceCount1Textfield = WorkPlaceCount1Panel.Find<UITextField>("WorkPlaceCount1TextField");
            var m_workPlaceCount2Textfield = WorkPlaceCount2Panel.Find<UITextField>("WorkPlaceCount2TextField");
            var m_workPlaceCount3Textfield = WorkPlaceCount3Panel.Find<UITextField>("WorkPlaceCount3TextField");

            // if set to default
            if(setDefault)
            {
                if(buildingAI is BarracksAI barracksAI)
                {
                    barracksAI = HousingManager.DefaultBarracksValues(barracksAI);
                    m_apartmentsNumTextfield.text = barracksAI.numApartments.ToString();
                    m_workPlaceCount0Textfield.text = barracksAI.m_workPlaceCount0.ToString();
                    m_workPlaceCount1Textfield.text = barracksAI.m_workPlaceCount1.ToString();
                    m_workPlaceCount2Textfield.text = barracksAI.m_workPlaceCount2.ToString();
                    m_workPlaceCount3Textfield.text = barracksAI.m_workPlaceCount3.ToString();
                }
                else if(buildingAI is DormsAI dormsAI)
                {
                    dormsAI = HousingManager.DefaultDormsValues(dormsAI);
                    m_apartmentsNumTextfield.text = dormsAI.numApartments.ToString();
                    m_workPlaceCount0Textfield.text = dormsAI.m_workPlaceCount0.ToString();
                    m_workPlaceCount1Textfield.text = dormsAI.m_workPlaceCount1.ToString();
                    m_workPlaceCount2Textfield.text = dormsAI.m_workPlaceCount2.ToString();
                    m_workPlaceCount3Textfield.text = dormsAI.m_workPlaceCount3.ToString();
                }
            }
           
            if(isBuilding)
            {
                var buildingRecord = HousingManager.GetBuildingRecord(buildingID);

                buildingRecord.NumOfApartments = int.Parse(m_apartmentsNumTextfield.text);
                buildingRecord.BuildingAI = BuildingAIstr;
                buildingRecord.WorkPlaceCount0 = int.Parse(m_workPlaceCount0Textfield.text);
                buildingRecord.WorkPlaceCount1 = int.Parse(m_workPlaceCount1Textfield.text);
                buildingRecord.WorkPlaceCount2 = int.Parse(m_workPlaceCount2Textfield.text);
                buildingRecord.WorkPlaceCount3 = int.Parse(m_workPlaceCount3Textfield.text);
                buildingRecord.DefaultValues = setDefault;

                HousingManager.SetBuildingRecord(buildingID, buildingRecord);
			}
			else if(isPrefab)
			{
                var prefabRecord = HousingManager.GetPrefab(buildingInfo.name, BuildingAIstr);

                prefabRecord.NumOfApartments = int.Parse(m_apartmentsNumTextfield.text);
                prefabRecord.BuildingAI = BuildingAIstr;
                prefabRecord.WorkPlaceCount0 = int.Parse(m_workPlaceCount0Textfield.text);
                prefabRecord.WorkPlaceCount1 = int.Parse(m_workPlaceCount1Textfield.text);
                prefabRecord.WorkPlaceCount2 = int.Parse(m_workPlaceCount2Textfield.text);
                prefabRecord.WorkPlaceCount3 = int.Parse(m_workPlaceCount3Textfield.text);

                HousingManager.SetPrefab(prefabRecord);
            }
            else if(isGlobal)
            {
                var housing = HousingConfig.Config.GetGlobalSettings(buildingInfo.name, BuildingAIstr);

                housing.Name = buildingInfo.name;
                housing.BuildingAI = BuildingAIstr;
                housing.NumOfApartments = int.Parse(m_apartmentsNumTextfield.text);
                housing.WorkPlaceCount0 = int.Parse(m_workPlaceCount0Textfield.text);
                housing.WorkPlaceCount1 = int.Parse(m_workPlaceCount1Textfield.text);
                housing.WorkPlaceCount2 = int.Parse(m_workPlaceCount2Textfield.text);
                housing.WorkPlaceCount3 = int.Parse(m_workPlaceCount3Textfield.text);

                HousingConfig.Config.SetGlobalSettings(housing);
            }

            RefreshData();
        }

        public static void ApplyPrefabSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ConfirmPanel.ShowModal("Apply Prefab Settings", "This will remove all building records of this type!", (comp, ret) =>
            {
                if (ret != 1)
                    return;
                SetPrefabGlobalSettings(false);
            });
        }

        public static void ApplyGlobalSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ConfirmPanel.ShowModal("Apply Global Settings", "This will remove all building records and prefab records of this type!", (comp, ret) =>
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

    }

}
