using CampusIndustriesHousingMod.AI;
using CampusIndustriesHousingMod.Utils;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;


namespace CampusIndustriesHousingMod.UI
{
    public static class HousingUIPanel
    {
        public static UIPanel m_uiMainPanel;

        private static CityServiceWorldInfoPanel m_cityServiceWorldInfoPanel;
        
        private static UILabel m_settingsHeader;
        private static UICheckBox m_settingsCheckBox;

        private static UIPanel ApartmentNumberPanel;
        private static UIPanel WorkPlaceCount0Panel;
        private static UIPanel WorkPlaceCount1Panel;
        private static UIPanel WorkPlaceCount2Panel;
        private static UIPanel WorkPlaceCount3Panel;

        private static UIButton ApplySettingsThisBuildingOnly;
        private static UIButton ApplySettingsThisBuildingTypeDefaultThisSave;
        private static UIButton ApplySettingsThisBuildingTypeDefaultGlobal;

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
                var default_height = 18f;
                m_uiMainPanel.height = 10f + (default_height * 12f/16f + 2f) * 25 + 10f;
                m_uiMainPanel.isVisible = HousingConfig.Config.ShowPanel;
                m_uiMainPanel.relativePosition = new Vector3(m_uiMainPanel.parent.width + 1f, 0f);

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
                    HousingConfig.Config.ShowPanel = value;
                    HousingConfig.Config.Serialize();
                };
                m_parkButtons.AttachUIComponent(m_settingsCheckBox.gameObject);

                m_settingsHeader = UiUtils.CreateLabel(m_uiMainPanel, "SettingsPanelHeader", "Settings", "");
                m_settingsHeader.font = UiUtils.GetUIFont("OpenSans-Regular");
                m_settingsHeader.textAlignment = UIHorizontalAlignment.Center;
                m_settingsHeader.relativePosition = new Vector3(10f, 60f + 0 * (default_height * 0.8f + 2f));

                ApartmentNumberPanel = UiUtils.UIServiceBar(m_uiMainPanel, "ApartmentNumber", "", "Number of apartments: ", "number of apartments");
                ApartmentNumberPanel.relativePosition = new Vector3(10f, 60f + 2 * (default_height * 0.8f + 2f));

                WorkPlaceCount0Panel = UiUtils.UIServiceBar(m_uiMainPanel, "WorkPlaceCount0", "", "Uneducated Workers: ", "number of uneducated workers");
                WorkPlaceCount0Panel.relativePosition = new Vector3(10f, 60f + 4 * (default_height * 0.8f + 2f));

                WorkPlaceCount1Panel = UiUtils.UIServiceBar(m_uiMainPanel, "WorkPlaceCount1", "", "Educated Workers: ", "number of educated workers");
                WorkPlaceCount1Panel.relativePosition = new Vector3(10f, 60f + 6 * (default_height * 0.8f + 2f));

                WorkPlaceCount2Panel = UiUtils.UIServiceBar(m_uiMainPanel, "WorkPlaceCount2", "", "Well Educated Workers: ", "number of well educated workers");
                WorkPlaceCount2Panel.relativePosition = new Vector3(10f, 60f + 8 * (default_height * 0.8f + 2f));

                WorkPlaceCount3Panel = UiUtils.UIServiceBar(m_uiMainPanel, "WorkPlaceCount3", "", "Highly Educated Workers: ", "number of highly educated workers");
                WorkPlaceCount3Panel.relativePosition = new Vector3(10f, 60f + 10 * (default_height * 0.8f + 2f));
 
                ApplySettingsThisBuildingOnly = UiUtils.CreateButton(m_uiMainPanel, "ApplySettingsThisBuildingOnly", "apply to building");
                ApplySettingsThisBuildingOnly.relativePosition = new Vector3(10f, 60f + 12 * (default_height * 0.8f + 2f));
                ApplySettingsThisBuildingOnly.eventClicked += SaveChangesThisBuildingOnly;

                ApplySettingsThisBuildingTypeDefaultThisSave = UiUtils.CreateButton(m_uiMainPanel, "ApplySettingsThisBuildingTypeDefaultThisSave", "apply to type across save");
                ApplySettingsThisBuildingTypeDefaultThisSave.relativePosition = new Vector3(10f, 60f + 14 * (default_height * 0.8f + 2f));
                ApplySettingsThisBuildingTypeDefaultThisSave.eventClicked += SaveChangesThisBuildingTypeDefaultThisSave;

                ApplySettingsThisBuildingTypeDefaultGlobal = UiUtils.CreateButton(m_uiMainPanel, "ApplySettingsThisBuildingTypeDefaultGlobal", "apply to type across all saves");
                ApplySettingsThisBuildingTypeDefaultGlobal.relativePosition = new Vector3(10f, 60f + 16 * (default_height * 0.8f + 2f));               
                ApplySettingsThisBuildingTypeDefaultGlobal.eventClicked += SaveChangesThisBuildingTypeDefaultGlobal;
            }
        }

        public static void RefreshData()
        {
            ushort buildingID = WorldInfoPanel.GetCurrentInstanceID().Building;
            BuildingInfo buildingInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info;
            var buildingAI = buildingInfo.GetAI();
            if (buildingAI is not BarracksAI && buildingAI is not DormsAI)
			{
                m_settingsCheckBox.Hide();
                m_uiMainPanel.Hide();
			}
            else
			{
                var type = buildingInfo.GetAI().ToString();
                var m_apartmentsNumTextfield = ApartmentNumberPanel.Find<UITextField>("ApartmentNumberTextField");
                var m_workPlaceCount0Textfield = WorkPlaceCount0Panel.Find<UITextField>("WorkPlaceCount0TextField");
                var m_workPlaceCount1Textfield = WorkPlaceCount1Panel.Find<UITextField>("WorkPlaceCount1TextField");
                var m_workPlaceCount2Textfield = WorkPlaceCount2Panel.Find<UITextField>("WorkPlaceCount2TextField");
                var m_workPlaceCount3Textfield = WorkPlaceCount3Panel.Find<UITextField>("WorkPlaceCount3TextField");
                

                var res = HousingManager.BuildingRecords.TryGetValue(buildingID, out HousingManager.BuildingRecord buildingRecord);
                if(res)
                {
                    m_apartmentsNumTextfield.text = buildingRecord.NumOfApartments.ToString();
                    m_workPlaceCount0Textfield.text = buildingRecord.WorkPlaceCount0.ToString();
                    m_workPlaceCount1Textfield.text = buildingRecord.WorkPlaceCount1.ToString();
                    m_workPlaceCount2Textfield.text = buildingRecord.WorkPlaceCount2.ToString();
                    m_workPlaceCount3Textfield.text = buildingRecord.WorkPlaceCount3.ToString();
                } 
                else
                {
                    var prefab_index = HousingManager.PrefabRecords.FindIndex(item => item.BuildingAI == type);
                    if(prefab_index != -1)
                    {
                        var defaultBarracksPrefab = HousingManager.PrefabRecords[prefab_index];
                        m_apartmentsNumTextfield.text = defaultBarracksPrefab.NumOfApartments.ToString();
                        m_workPlaceCount0Textfield.text = defaultBarracksPrefab.WorkPlaceCount0.ToString();
                        m_workPlaceCount1Textfield.text = defaultBarracksPrefab.WorkPlaceCount1.ToString();
                        m_workPlaceCount2Textfield.text = defaultBarracksPrefab.WorkPlaceCount2.ToString();
                        m_workPlaceCount3Textfield.text = defaultBarracksPrefab.WorkPlaceCount3.ToString();
                    }
                    else
                    {
                        var global_index = HousingConfig.Config.HousingSettings.FindIndex(item => item.Name == buildingInfo.name && item.BuildingAI == type);
                        if(global_index != -1)
                        {
                            var saved_config = HousingConfig.Config.HousingSettings[global_index];
                            m_apartmentsNumTextfield.text = saved_config.NumOfApartments.ToString();
                            m_workPlaceCount0Textfield.text = saved_config.WorkPlaceCount0.ToString();
                            m_workPlaceCount1Textfield.text = saved_config.WorkPlaceCount1.ToString();
                            m_workPlaceCount2Textfield.text = saved_config.WorkPlaceCount2.ToString();
                            m_workPlaceCount3Textfield.text = saved_config.WorkPlaceCount3.ToString();
                        }
                        else
                        {
                            if(type == "BarracksAI")
                            {
                                BarracksAI barracksAI = buildingAI as BarracksAI;
                                barracksAI = HousingManager.DefaultBarracksValues(barracksAI);
                                m_apartmentsNumTextfield.text = barracksAI.numApartments.ToString();
                                m_workPlaceCount0Textfield.text = barracksAI.m_workPlaceCount0.ToString();
                                m_workPlaceCount1Textfield.text = barracksAI.m_workPlaceCount1.ToString();
                                m_workPlaceCount2Textfield.text = barracksAI.m_workPlaceCount2.ToString();
                                m_workPlaceCount3Textfield.text = barracksAI.m_workPlaceCount3.ToString();
                            }
                            else if(type == "DormsAI")
                            {
                                DormsAI dormsAI = buildingAI as DormsAI;
                                dormsAI = HousingManager.DefaultDormsValues(dormsAI);
                                m_apartmentsNumTextfield.text = dormsAI.numApartments.ToString();
                                m_workPlaceCount0Textfield.text = dormsAI.m_workPlaceCount0.ToString();
                                m_workPlaceCount1Textfield.text = dormsAI.m_workPlaceCount1.ToString();
                                m_workPlaceCount2Textfield.text = dormsAI.m_workPlaceCount2.ToString();
                                m_workPlaceCount3Textfield.text = dormsAI.m_workPlaceCount3.ToString();
                            }
                        }
                    }
                }
                m_settingsCheckBox.Show();
			}
        }

        public static void SaveChangesThisBuildingOnly(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ushort buildingID = WorldInfoPanel.GetCurrentInstanceID().Building;
            BuildingInfo buildingInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info;

            HousingManager.BuildingRecord buildingRecord = new();

            var buildingAI = buildingInfo.GetAI();

            var m_apartmentsNumTextfield = ApartmentNumberPanel.Find<UITextField>("ApartmentNumberTextField");
            var m_workPlaceCount0Textfield = WorkPlaceCount0Panel.Find<UITextField>("WorkPlaceCount0TextField");
            var m_workPlaceCount1Textfield = WorkPlaceCount1Panel.Find<UITextField>("WorkPlaceCount1TextField");
            var m_workPlaceCount2Textfield = WorkPlaceCount2Panel.Find<UITextField>("WorkPlaceCount2TextField");
            var m_workPlaceCount3Textfield = WorkPlaceCount3Panel.Find<UITextField>("WorkPlaceCount3TextField");

            buildingRecord.NumOfApartments = int.Parse(m_apartmentsNumTextfield.text);
            buildingRecord.WorkPlaceCount0 = int.Parse(m_workPlaceCount0Textfield.text);
            buildingRecord.WorkPlaceCount1 = int.Parse(m_workPlaceCount1Textfield.text);
            buildingRecord.WorkPlaceCount2 = int.Parse(m_workPlaceCount2Textfield.text);
            buildingRecord.WorkPlaceCount3 = int.Parse(m_workPlaceCount3Textfield.text);

            if(buildingAI is BarracksAI)
            {
                buildingRecord.BuildingAI = "BarracksAI";
            }
            else if(buildingAI is DormsAI)
            {
                buildingRecord.BuildingAI = "DormsAI";
            }
            HousingManager.AddBuilding(buildingID, buildingRecord);
        }

        public static void SaveChangesThisBuildingTypeDefaultThisSave(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ushort buildingID = WorldInfoPanel.GetCurrentInstanceID().Building;
            BuildingInfo buildingInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info;

            HousingManager.PrefabRecord prefabRecord = new();

            var m_apartmentsNumTextfield = ApartmentNumberPanel.Find<UITextField>("ApartmentNumberTextField");
            var m_workPlaceCount0Textfield = WorkPlaceCount0Panel.Find<UITextField>("WorkPlaceCount0TextField");
            var m_workPlaceCount1Textfield = WorkPlaceCount1Panel.Find<UITextField>("WorkPlaceCount1TextField");
            var m_workPlaceCount2Textfield = WorkPlaceCount2Panel.Find<UITextField>("WorkPlaceCount2TextField");
            var m_workPlaceCount3Textfield = WorkPlaceCount3Panel.Find<UITextField>("WorkPlaceCount3TextField");

            prefabRecord.Name = buildingInfo.name;
            prefabRecord.NumOfApartments = int.Parse(m_apartmentsNumTextfield.text);
            prefabRecord.WorkPlaceCount0 = int.Parse(m_workPlaceCount0Textfield.text);
            prefabRecord.WorkPlaceCount1 = int.Parse(m_workPlaceCount1Textfield.text);
            prefabRecord.WorkPlaceCount2 = int.Parse(m_workPlaceCount2Textfield.text);
            prefabRecord.WorkPlaceCount3 = int.Parse(m_workPlaceCount3Textfield.text);

            var buildingAI = buildingInfo.GetAI();

            if(buildingAI is BarracksAI)
            {
                prefabRecord.BuildingAI = "BarracksAI";
            }
            else if(buildingAI is DormsAI)
            {
                prefabRecord.BuildingAI = "DormsAI";
            }

            HousingManager.AddPrefab(prefabRecord);
        }

        public static void SaveChangesThisBuildingTypeDefaultGlobal(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ushort buildingID = WorldInfoPanel.GetCurrentInstanceID().Building;
            BuildingInfo buildingInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info;

            Housing housing = new();

            var m_apartmentsNumTextfield = ApartmentNumberPanel.GetComponent<UITextField>();
            var m_workPlaceCount0Textfield = WorkPlaceCount0Panel.GetComponent<UITextField>();
            var m_workPlaceCount1Textfield = WorkPlaceCount1Panel.GetComponent<UITextField>();
            var m_workPlaceCount2Textfield = WorkPlaceCount2Panel.GetComponent<UITextField>();
            var m_workPlaceCount3Textfield = WorkPlaceCount3Panel.GetComponent<UITextField>();

            housing.Name = buildingInfo.name;
            housing.NumOfApartments = int.Parse(m_apartmentsNumTextfield.text);
            housing.WorkPlaceCount0 = int.Parse(m_workPlaceCount0Textfield.text);
            housing.WorkPlaceCount1 = int.Parse(m_workPlaceCount1Textfield.text);
            housing.WorkPlaceCount2 = int.Parse(m_workPlaceCount2Textfield.text);
            housing.WorkPlaceCount3 = int.Parse(m_workPlaceCount3Textfield.text);

            var buildingAI = buildingInfo.GetAI();

            if(buildingAI is BarracksAI)
            {
                housing.BuildingAI = "BarracksAI";
            }
            else if(buildingAI is DormsAI)
            {
                housing.BuildingAI = "DormsAI";
            }

            HousingConfig.Config.AddGlobalSettings(housing);
        }

    }

}
