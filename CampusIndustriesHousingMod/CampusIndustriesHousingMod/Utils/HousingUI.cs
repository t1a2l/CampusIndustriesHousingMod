using System;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using static CampusIndustriesHousingMod.HousingManager;

namespace CampusIndustriesHousingMod
{
    public class HousingUI
    {
        CityServiceWorldInfoPanel _cityServiceWorldInfoPanel;
        UIPanel _parkButtonsPanel;
        UIComponent parkButtons;
        UIComponent wrapper;
        UIComponent mainSectionPanel;
        UIComponent mainBottom;
        UIComponent buttonPanels;

        private UILabel _settingsHeader;
        private UIPanel _settingsPanel;
        private UICheckBox _settingsCheckBox;

        private UILabel _apartmentsNumLabel;
        private static UITextField _apartmentsNumTextfield;

        private UILabel _workPlaceCount0Label;
        private static UITextField _workPlaceCount0Textfield;

        private UILabel _workPlaceCount1Label;
        private static UITextField _workPlaceCount1Textfield;

        private UILabel _workPlaceCount2Label;
        private static UITextField _workPlaceCount2Textfield;

        private UILabel _workPlaceCount3Label;
        private static UITextField _workPlaceCount3Textfield;

        private UIButton ApplySettingsThisBuildingOnly;
        private UIButton ApplySettingsThisBuildingTypeDefaultThisSave;
        private UIButton ApplySettingsThisBuildingTypeDefaultGlobal;

        public void Start()
        {
            try
            {
                _cityServiceWorldInfoPanel = GameObject.Find("(Library) CityServiceWorldInfoPanel").GetComponent<CityServiceWorldInfoPanel>();

                // Get ParkButtons UIPanel.
                wrapper = _cityServiceWorldInfoPanel?.Find("Wrapper");
                mainSectionPanel = wrapper?.Find("MainSectionPanel");
                mainBottom = mainSectionPanel?.Find("MainBottom");
                buttonPanels = mainSectionPanel?.Find("ButtonPanels");
                parkButtons = mainSectionPanel?.Find("ParkButtons");

                _parkButtonsPanel = _cityServiceWorldInfoPanel.Find("ParkButtons").GetComponent<UIPanel>();

                CreateUI();
            }
            catch (Exception e)
            {
                Debug.Log("[Show It!] ModManager:Start -> Exception: " + e.Message);
            }
        }

        public void CreateUI()
        {
            try
            {  
                ushort buildingID = WorldInfoPanel.GetCurrentInstanceID().Building;

                if(parkButtons != null)
                {
                    _settingsPanel = CreatePanel(_cityServiceWorldInfoPanel.component, "ShowSettingsPanel");
                    _settingsPanel.opacity = 0.90f;

                    _settingsCheckBox = CreateCheckBox(parkButtons, "SettingsCheckBox", "settings", false);
                    _settingsCheckBox.width = 110f;
                    _settingsCheckBox.label.textColor = new Color32(185, 221, 254, 255);
                    _settingsCheckBox.label.textScale = 0.8125f;
                    _settingsCheckBox.tooltip = "Indicators will show how well serviced the building is and what problems might prevent the building from leveling up.";
                    _settingsCheckBox.AlignTo(_parkButtonsPanel, UIAlignAnchor.TopLeft);
                    _settingsCheckBox.relativePosition = new Vector3(_parkButtonsPanel.width - _settingsCheckBox.width, 6f);
                    _settingsCheckBox.eventCheckChanged += (component, value) =>
                    {
                        _settingsCheckBox.isVisible = value;
                    };

                    _settingsHeader = CreateLabel(_settingsPanel, "SettingsPanelHeader", "Settings", "");
                    _settingsHeader.font = GetUIFont("OpenSans-Regular");
                    _settingsHeader.textAlignment = UIHorizontalAlignment.Center;

                    _apartmentsNumLabel = CreateLabel(_settingsPanel, "ApartmentNumberLabel", "", "Number of apartments: ");
                    _apartmentsNumLabel.relativePosition = PositionUnder(_settingsHeader);
                    _apartmentsNumTextfield = CreateTextField(_settingsPanel, "ApartmentNumberTextfield", "number of apartments");
                    _apartmentsNumTextfield.relativePosition = PositionRightOf(_apartmentsNumLabel);

                    _workPlaceCount0Label = CreateLabel(_settingsPanel, "WorkPlaceCount0Label", "", "Uneducated Workers: ");
                    _workPlaceCount0Label.relativePosition = PositionUnder(_apartmentsNumLabel);
                    _workPlaceCount0Textfield = CreateTextField(_settingsPanel, "WorkPlaceCount0Textfield", "number of uneducated workers");
                    _workPlaceCount0Textfield.relativePosition = PositionRightOf(_workPlaceCount0Label);

                    _workPlaceCount1Label = CreateLabel(_settingsPanel, "WorkPlaceCount1Label", "", "Educated Workers: ");
                    _workPlaceCount1Label.relativePosition = PositionUnder(_workPlaceCount0Label);
                    _workPlaceCount1Textfield = CreateTextField(_settingsPanel, "WorkPlaceCount1Textfield", "number of educated workers");
                    _workPlaceCount1Textfield.relativePosition = PositionRightOf(_workPlaceCount1Label);

                     _workPlaceCount2Label = CreateLabel(_settingsPanel, "WorkPlaceCount2Label", "", "Well Educated Workers: ");
                    _workPlaceCount2Label.relativePosition = PositionUnder(_workPlaceCount1Label);
                    _workPlaceCount2Textfield = CreateTextField(_settingsPanel, "WorkPlaceCount0Textfield", "number of well educated workers");
                    _workPlaceCount2Textfield.relativePosition = PositionRightOf(_workPlaceCount2Label);

                     _workPlaceCount3Label = CreateLabel(_settingsPanel, "WorkPlaceCount3Label", "", "Highly Educated Workers: ");
                    _workPlaceCount3Label.relativePosition = PositionUnder(_workPlaceCount2Label);
                    _workPlaceCount3Textfield = CreateTextField(_settingsPanel, "WorkPlaceCount0Textfield", "number of highly educated workers");
                    _workPlaceCount3Textfield.relativePosition = PositionRightOf(_workPlaceCount3Label);

                    // set default values
                    Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
                    BuildingInfo buildingInfo = buildingBuffer[buildingID].Info;
                    var buildingAI = buildingInfo.GetAI();
                    if (buildingAI == null)
                    {
                        // Not a barracks or dorms building - hide the label.
                        _settingsPanel.Hide();
                    }
                    else
                    {
                        var res = HousingManager.BuildingRecords.TryGetValue(buildingID, out BuildingRecord buildingRecord);
                        if(res)
                        {
                            _apartmentsNumTextfield.text = buildingRecord.NumOfApartments.ToString();
                            _workPlaceCount0Textfield.text = buildingRecord.WorkPlaceCount0.ToString();
                            _workPlaceCount1Textfield.text = buildingRecord.WorkPlaceCount1.ToString();
                            _workPlaceCount2Textfield.text = buildingRecord.WorkPlaceCount2.ToString();
                            _workPlaceCount3Textfield.text = buildingRecord.WorkPlaceCount3.ToString();
                        } 
                        else
                        {

                            if(buildingAI is BarracksAI barracksAI)
                            {
                                barracksAI = DefaultBarracksValues(barracksAI);
                                _apartmentsNumTextfield.text = barracksAI.numApartments.ToString();
                                _workPlaceCount0Textfield.text = barracksAI.m_workPlaceCount0.ToString();
                                _workPlaceCount1Textfield.text = barracksAI.m_workPlaceCount1.ToString();
                                _workPlaceCount2Textfield.text = barracksAI.m_workPlaceCount2.ToString();
                                _workPlaceCount3Textfield.text = barracksAI.m_workPlaceCount3.ToString();
                            }
                            else if(buildingAI is DormsAI dormsAI)
                            {
                                dormsAI = DefaultDormsValues(dormsAI);
                                _apartmentsNumTextfield.text = dormsAI.numApartments.ToString();
                                _workPlaceCount0Textfield.text = dormsAI.m_workPlaceCount0.ToString();
                                _workPlaceCount1Textfield.text = dormsAI.m_workPlaceCount1.ToString();
                                _workPlaceCount2Textfield.text = dormsAI.m_workPlaceCount2.ToString();
                                _workPlaceCount3Textfield.text = dormsAI.m_workPlaceCount3.ToString();
                            }
                        }
                        _settingsPanel.Show();
                    }

                    ApplySettingsThisBuildingOnly = CreateButton(_settingsPanel, "apply settings to this building");
                    ApplySettingsThisBuildingOnly.relativePosition = PositionUnder(_workPlaceCount3Label);
                    ApplySettingsThisBuildingOnly.eventClicked += delegate { SaveChangesThisBuildingOnly(buildingID, buildingInfo); };

                    ApplySettingsThisBuildingTypeDefaultThisSave = CreateButton(_settingsPanel, "set default type values");
                    ApplySettingsThisBuildingTypeDefaultThisSave.relativePosition = PositionRightOf(ApplySettingsThisBuildingOnly);
                    ApplySettingsThisBuildingTypeDefaultThisSave.eventClicked += delegate { SaveChangesThisBuildingTypeDefaultThisSave(buildingInfo); };

                    ApplySettingsThisBuildingTypeDefaultGlobal = CreateButton(_settingsPanel, "set default global values");
                    ApplySettingsThisBuildingTypeDefaultGlobal.relativePosition = PositionRightOf(ApplySettingsThisBuildingTypeDefaultThisSave);
                    ApplySettingsThisBuildingTypeDefaultGlobal.eventClicked += delegate { SaveChangesThisBuildingTypeDefaultGlobal(buildingInfo); };
                }
            }
            catch (Exception e)
            {
                Debug.Log("Exception: " + e.Message);
            }
        }


        public static void SaveChangesThisBuildingOnly(ushort buildingID, BuildingInfo buildingInfo)
        {
            BuildingRecord buildingRecord = new();

            var buildingAI = buildingInfo.GetAI();

            buildingRecord.NumOfApartments = int.Parse(_apartmentsNumTextfield.text);
            buildingRecord.WorkPlaceCount0 = int.Parse(_workPlaceCount0Textfield.text);
            buildingRecord.WorkPlaceCount1 = int.Parse(_workPlaceCount1Textfield.text);
            buildingRecord.WorkPlaceCount2 = int.Parse(_workPlaceCount2Textfield.text);
            buildingRecord.WorkPlaceCount3 = int.Parse(_workPlaceCount3Textfield.text);

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

        public static void SaveChangesThisBuildingTypeDefaultThisSave(BuildingInfo buildingInfo)
        {
            PrefabRecord prefabRecord = new();

            prefabRecord.Name = buildingInfo.name;
            prefabRecord.NumOfApartments = int.Parse(_apartmentsNumTextfield.text);
            prefabRecord.WorkPlaceCount0 = int.Parse(_workPlaceCount0Textfield.text);
            prefabRecord.WorkPlaceCount1 = int.Parse(_workPlaceCount1Textfield.text);
            prefabRecord.WorkPlaceCount2 = int.Parse(_workPlaceCount2Textfield.text);
            prefabRecord.WorkPlaceCount3 = int.Parse(_workPlaceCount3Textfield.text);

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

        public static void SaveChangesThisBuildingTypeDefaultGlobal(BuildingInfo buildingInfo)
        {
            Housing housing = new();

            housing.Name = buildingInfo.name;
            housing.NumOfApartments = int.Parse(_apartmentsNumTextfield.text);
            housing.WorkPlaceCount0 = int.Parse(_workPlaceCount0Textfield.text);
            housing.WorkPlaceCount1 = int.Parse(_workPlaceCount1Textfield.text);
            housing.WorkPlaceCount2 = int.Parse(_workPlaceCount2Textfield.text);
            housing.WorkPlaceCount3 = int.Parse(_workPlaceCount3Textfield.text);

            var buildingAI = buildingInfo.GetAI();

            if(buildingAI is BarracksAI)
            {
                housing.Type = "BarracksAI";
            }
            else if(buildingAI is DormsAI)
            {
                housing.Type = "DormsAI";
            }

            HousingConfig.Config.AddGlobalSettings(housing);
        }

        public static UIButton CreateButton(UIComponent parent, string name)
        {
            UIButton button = parent.AddUIComponent<UIButton>();
            button.name = name;
            button.autoSize = true;
            return button;
        }

        public static UICheckBox CreateCheckBox(UIComponent parent, string name, string text, bool state)
        {
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();
            checkBox.name = name;

            checkBox.height = 16f;
            checkBox.width = parent.width - 10f;

            UISprite uncheckedSprite = checkBox.AddUIComponent<UISprite>();
            uncheckedSprite.spriteName = "check-unchecked";
            uncheckedSprite.size = new Vector2(16f, 16f);
            uncheckedSprite.relativePosition = Vector3.zero;

            UISprite checkedSprite = checkBox.AddUIComponent<UISprite>();
            checkedSprite.spriteName = "check-checked";
            checkedSprite.size = new Vector2(16f, 16f);
            checkedSprite.relativePosition = Vector3.zero;
            checkBox.checkedBoxObject = checkedSprite;

            checkBox.label = checkBox.AddUIComponent<UILabel>();
            checkBox.label.text = text;
            checkBox.label.font = GetUIFont("OpenSans-Regular");
            checkBox.label.autoSize = false;
            checkBox.label.height = 20f;
            checkBox.label.verticalAlignment = UIVerticalAlignment.Middle;
            checkBox.label.relativePosition = new Vector3(20f, 0f);

            checkBox.isChecked = state;

            return checkBox;
        }

        public static UIFont GetUIFont(string name)
        {
            UIFont[] fonts = Resources.FindObjectsOfTypeAll<UIFont>();

            foreach (UIFont font in fonts)
            {
                if (font.name.CompareTo(name) == 0)
                {
                    return font;
                }
            }

            return null;
        }

        public static UILabel CreateLabel(UIComponent parent, string name, string text, string prefix)
        {
            UILabel label = parent.AddUIComponent<UILabel>();
            label.name = name;
            label.text = text;
            label.prefix = prefix;

            return label;
        }

        public static UIPanel CreatePanel(UIComponent parent, string name)
        {
            UIPanel panel = parent.AddUIComponent<UIPanel>();
            panel.name = name;

            return panel;
        }

        public static UITextField CreateTextField(UIComponent parent, string name, string tooltip)
		{
			UITextField textField = parent.AddUIComponent<UITextField>();
            textField.name = name;
            textField.size = new Vector2(90f, 17f);
            textField.padding = new RectOffset(0, 0, 9, 3);
            textField.builtinKeyNavigation = true;
            textField.isInteractive = true;
            textField.readOnly = false;
            textField.horizontalAlignment = UIHorizontalAlignment.Center;
            textField.verticalAlignment = UIVerticalAlignment.Middle;
            textField.selectionSprite = "EmptySprite";
            textField.selectionBackgroundColor = new Color32(233, 201, 148, 255);
            textField.normalBgSprite = "TextFieldPanelHovered";
            textField.disabledBgSprite = "TextFieldPanel";
            textField.textColor = new Color32(0, 0, 0, 255);
            textField.disabledTextColor = new Color32(0, 0, 0, 128);
            textField.color = new Color32(185, 221, 254, 255);
            textField.tooltip = tooltip;
            textField.size = new Vector2(150f, 27f);
            textField.padding.top = 2;
            textField.numericalOnly = true;
            textField.allowNegative = false;
            textField.allowFloats = false;
            textField.multiline = false;

            return textField;
		}

        private static Vector3 PositionUnder(UIComponent uIComponent, float margin = 8f, float horizontalOffset = 0f)
        {
            return new Vector3(uIComponent.relativePosition.x + horizontalOffset, uIComponent.relativePosition.y + uIComponent.height + margin);
        }

        private static Vector3 PositionRightOf(UIComponent uIComponent, float margin = 8f, float verticalOffset = 0f)
        {
            return new Vector3(uIComponent.relativePosition.x + uIComponent.width + margin, uIComponent.relativePosition.y + verticalOffset);
        }

        public static BarracksAI DefaultBarracksValues(BarracksAI barracks)
        {
            if(barracks.m_industryType == DistrictPark.ParkType.Farming)
            {
                barracks.numApartments = 2;
                barracks.m_workPlaceCount0 = 5;
                barracks.m_workPlaceCount1 = 0;
                barracks.m_workPlaceCount2 = 0;
                barracks.m_workPlaceCount3 = 0;
            }
            else if(barracks.m_industryType == DistrictPark.ParkType.Forestry)
            {
                barracks.numApartments = 10;
                barracks.m_workPlaceCount0 = 5;
                barracks.m_workPlaceCount1 = 2;
                barracks.m_workPlaceCount2 = 0;
                barracks.m_workPlaceCount3 = 0;
            }
            else if(barracks.m_industryType == DistrictPark.ParkType.Oil)
            {
                barracks.numApartments = 50;
                barracks.m_workPlaceCount0 = 5;
                barracks.m_workPlaceCount1 = 2;
                barracks.m_workPlaceCount2 = 0;
                barracks.m_workPlaceCount3 = 0;
            }
            else if(barracks.m_industryType == DistrictPark.ParkType.Ore)
            {
                barracks.numApartments = 48;
                barracks.m_workPlaceCount0 = 5;
                barracks.m_workPlaceCount1 = 2;
                barracks.m_workPlaceCount2 = 0;
                barracks.m_workPlaceCount3 = 0;
            }

            return barracks;
        }

        public static DormsAI DefaultDormsValues(DormsAI dorms)
        {
            if(dorms.m_campusType == DistrictPark.ParkType.University)
            {
                dorms.numApartments = 60;
                dorms.m_workPlaceCount0 = 3;
                dorms.m_workPlaceCount1 = 3;
                dorms.m_workPlaceCount2 = 0;
                dorms.m_workPlaceCount3 = 0;
            }
            else if(dorms.m_campusType == DistrictPark.ParkType.LiberalArts)
            {
                dorms.numApartments = 60;
                dorms.m_workPlaceCount0 = 3;
                dorms.m_workPlaceCount1 = 3;
                dorms.m_workPlaceCount2 = 0;
                dorms.m_workPlaceCount3 = 0;
            }
            else if(dorms.m_campusType == DistrictPark.ParkType.TradeSchool)
            {
                dorms.numApartments = 60;
                dorms.m_workPlaceCount0 = 3;
                dorms.m_workPlaceCount1 = 3;
                dorms.m_workPlaceCount2 = 0;
                dorms.m_workPlaceCount3 = 0;
            }

            return dorms;
        }

    }

}
