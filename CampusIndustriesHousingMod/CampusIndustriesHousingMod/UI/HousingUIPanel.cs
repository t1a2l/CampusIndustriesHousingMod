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
    public class HousingUIPanel
    {
        public readonly UIPanel m_uiMainPanel;
        
        private readonly UILabel m_settingsHeader;
        private readonly UILabel m_settingsStatus;
        private readonly UICheckBox m_settingsCheckBox;

        private readonly UIPanel ApartmentNumberPanel;

        private readonly UIButton SaveBuildingSettingsBtn;
        private readonly UIButton ReturnToDefaultBtn;

        private readonly UIButton ApplyPrefabSettingsBtn;
        private readonly UIButton ApplyGlobalSettingsBtn;

        private readonly UIButton SetPrefabSettingsBtn;
        private readonly UIButton SetGlobalSettingsBtn;

        private readonly UIButton UnlockSettingsBtn;

        private readonly float DEFAULT_HEIGHT = 18F;

        public HousingUIPanel(BuildingWorldInfoPanel buildingWorldInfoPanel, UIPanel uIPanel)
        {
            m_uiMainPanel = buildingWorldInfoPanel.component.AddUIComponent<UIPanel>();
            m_uiMainPanel.name = "HousingUIPanel";
            m_uiMainPanel.backgroundSprite = "SubcategoriesPanel";
            m_uiMainPanel.opacity = 0.90f;
            m_uiMainPanel.isVisible = HousingConfig.Config.ShowPanel;
            m_uiMainPanel.relativePosition = new Vector3(m_uiMainPanel.parent.width + 1f, 40f);
            m_uiMainPanel.height = 370f;
            m_uiMainPanel.width = 510f;

            m_settingsCheckBox = UiUtils.CreateCheckBox(uIPanel, "SettingsCheckBox", "settings", HousingConfig.Config.ShowPanel);
            m_settingsCheckBox.width = 110f;
            m_settingsCheckBox.label.textColor = new Color32(185, 221, 254, 255);
            m_settingsCheckBox.label.textScale = 0.8125f;
            m_settingsCheckBox.tooltip = "Indicators will show how well serviced the building is and what problems might prevent the building from leveling up.";
            m_settingsCheckBox.AlignTo(buildingWorldInfoPanel.component, UIAlignAnchor.TopLeft);
            m_settingsCheckBox.relativePosition = new Vector3(m_uiMainPanel.width - m_settingsCheckBox.width, 6f);
            m_settingsCheckBox.eventCheckChanged += (component, value) =>
            {
                m_uiMainPanel.isVisible = value;
                m_uiMainPanel.height = 370f;
                HousingConfig.Config.ShowPanel = value;
                if(!value)
                {
                    SaveBuildingSettingsBtn.Disable();
                    ReturnToDefaultBtn.Disable();
                    ApplyPrefabSettingsBtn.Disable();
                    ApplyGlobalSettingsBtn.Disable();
                    SetPrefabSettingsBtn.Disable();
                    SetGlobalSettingsBtn.Disable();
                    UnlockSettingsBtn.Show();
                }
                HousingConfig.Config.Serialize();
            };
            uIPanel.AttachUIComponent(m_settingsCheckBox.gameObject);

            m_settingsHeader = UiUtils.CreateLabel(m_uiMainPanel, "SettingsPanelHeader", "Settings", "");
            m_settingsHeader.font = UiUtils.GetUIFont("OpenSans-Regular");
            m_settingsHeader.textAlignment = UIHorizontalAlignment.Center;
            m_settingsHeader.relativePosition = new Vector3(10f, 60f + 0 * (DEFAULT_HEIGHT * 0.8f + 2f));

            m_settingsStatus = UiUtils.CreateLabel(m_uiMainPanel, "SettingsStatus", "", "");
            m_settingsStatus.font = UiUtils.GetUIFont("OpenSans-Regular");
            m_settingsStatus.textAlignment = UIHorizontalAlignment.Center;
            m_settingsStatus.textColor = new Color32(240, 190, 199, 255);
            m_settingsStatus.relativePosition = new Vector3(100f, 20f);
            m_settingsStatus.textScale = 1.2f;

            ApartmentNumberPanel = UiUtils.UIServiceBar(m_uiMainPanel, "ApartmentNumber", "", "Number of apartments: ", "number of apartments");
            ApartmentNumberPanel.relativePosition = new Vector3(10f, 60f + 2 * (DEFAULT_HEIGHT * 0.8f + 2f));

            SaveBuildingSettingsBtn = UiUtils.AddButton(m_uiMainPanel, 260f, 60f + 2 * (DEFAULT_HEIGHT * 0.8f + 2f), "SaveBuildingSettings", "Save building settings", "First priority - will override prefab and global settings create a record for this building");
            SaveBuildingSettingsBtn.eventClicked += SaveBuildingSettings;

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

            SaveBuildingSettingsBtn.Disable();
            ReturnToDefaultBtn.Disable();
            ApplyPrefabSettingsBtn.Disable();
            ApplyGlobalSettingsBtn.Disable();
            SetPrefabSettingsBtn.Disable();
            SetGlobalSettingsBtn.Disable();
        }

        public void UnlockSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            SaveBuildingSettingsBtn.Enable();
            ReturnToDefaultBtn.Enable();

            ushort buildingID = WorldInfoPanel.GetCurrentInstanceID().Building;
            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID];

            if (HousingManager.PrefabExist(building.Info))
            {
                ApplyPrefabSettingsBtn.Enable();
            }

            if (HousingConfig.Config.GetGlobalSettings(building.Info) != null)
            {
                ApplyGlobalSettingsBtn.Enable();
            }

            SetPrefabSettingsBtn.Enable();
            SetGlobalSettingsBtn.Enable();

            UnlockSettingsBtn.Hide();
        }

        public void RefreshData()
        {
            ushort buildingID = WorldInfoPanel.GetCurrentInstanceID().Building;
            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID];
            PrefabAI buildingAI = building.Info.GetAI();

            if (buildingAI is BarracksAI || buildingAI is DormsAI)
			{
                int numOfApartments = 0;
                var m_apartmentsNumTextfield = ApartmentNumberPanel.Find<UITextField>("ApartmentNumberTextField");

                HousingManager.BuildingRecord buildingRecord;

                if (!HousingManager.BuildingRecordExist(buildingID))
                {
                    buildingRecord = HousingManager.CreateBuildingRecord(buildingID);
                }
                else
                {
                    buildingRecord = HousingManager.GetBuildingRecord(buildingID);
                }

                var globalRecord = HousingConfig.Config.GetGlobalSettings(building.Info);

                if (!buildingRecord.IsPrefab && !buildingRecord.IsGlobal)
                {
                    m_settingsStatus.text = buildingRecord.IsDefault ? "This Building is using default settings" : "This Building is using his own settings";
                    m_apartmentsNumTextfield.text = buildingRecord.NumOfApartments.ToString();
                    numOfApartments = buildingRecord.NumOfApartments;
                }
                else if (HousingManager.PrefabExist(building.Info) && buildingRecord.IsPrefab)
                {
                    m_settingsStatus.text = "This Building is using type settings";

                    var prefabRecord = HousingManager.GetPrefab(building.Info);

                    m_apartmentsNumTextfield.text = prefabRecord.NumOfApartments.ToString();
                    numOfApartments = prefabRecord.NumOfApartments;
                }
                else if(globalRecord != null && buildingRecord.IsGlobal)
                {
                    m_settingsStatus.text = "This Building is using global settings";
                    m_apartmentsNumTextfield.text = globalRecord.NumOfApartments.ToString();
                    numOfApartments = globalRecord.NumOfApartments;
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
            else 
            {
                m_settingsCheckBox.Hide();
                m_uiMainPanel.Hide();
            }
        }

        public void SaveBuildingSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ApplySettings(true, false, false);
        }
        
        public void ReturnToDefault(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ApplySettings(false, false, false);
        }

        public void ApplyPrefabSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ApplySettings(false, true, false);
        }

        public void ApplyGlobalSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ApplySettings(false, false, true);
        }

        private void UpdateHouse(ushort buildingID, ref Building data, int numOfApartments)
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

        private void EnsureCitizenUnits(ushort buildingID, ref Building data, int homeCount = 0, int workCount = 0, int visitCount = 0, int studentCount = 0, int hotelCount = 0)
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

        private void ApplySettings(bool isBuilding, bool isPrefab, bool isGlobal)
        {
            ushort buildingID = WorldInfoPanel.GetCurrentInstanceID().Building;
            BuildingInfo buildingInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info;
            var buildingAI = buildingInfo.GetAI();

            var m_apartmentsNumTextfield = ApartmentNumberPanel.Find<UITextField>("ApartmentNumberTextField");

            HousingManager.BuildingRecord buildingRecord;

            if (!HousingManager.BuildingRecordExist(buildingID))
            {
                buildingRecord = HousingManager.CreateBuildingRecord(buildingID);
            }
            else
            {
                buildingRecord = HousingManager.GetBuildingRecord(buildingID);
            }
           
            if(isBuilding)
            {
                string buildingAIstr = buildingAI.GetType().Name;
                buildingRecord.NumOfApartments = int.Parse(m_apartmentsNumTextfield.text);
                buildingRecord.BuildingAI = buildingAIstr;
                buildingRecord.IsDefault = false;
                buildingRecord.IsPrefab = false;
                buildingRecord.IsGlobal = false;
            }
			else if(isPrefab && HousingManager.PrefabExist(buildingInfo))
			{
                var prefabRecord = HousingManager.GetPrefab(buildingInfo);
                buildingRecord.NumOfApartments = prefabRecord.NumOfApartments;
            }
            else if(isGlobal)
            {
                var housingGlobal = HousingConfig.Config.GetGlobalSettings(buildingInfo);

                if (housingGlobal != null)
                {
                    buildingRecord.NumOfApartments = housingGlobal.NumOfApartments;
                }
            }
            else
            {
                if (buildingAI is BarracksAI barracksAI)
                {
                    barracksAI = HousingManager.DefaultBarracksValues(barracksAI);
                    buildingRecord.NumOfApartments = barracksAI.numApartments;
                }
                else if (buildingAI is DormsAI dormsAI)
                {
                    dormsAI = HousingManager.DefaultDormsValues(dormsAI);
                    buildingRecord.NumOfApartments = dormsAI.numApartments;
                }
            }

            m_apartmentsNumTextfield.text = buildingRecord.NumOfApartments.ToString();

            RefreshData();
        }

        public void SetPrefabSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ConfirmPanel.ShowModal("Set Type Settings", "This will update all building records of this type to the current number of apartments in this save!", (comp, ret) =>
            {
                if (ret != 1)
                    return;
                SetPrefabGlobalSettings(false);
            });
        }

        public void SetGlobalSettings(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ConfirmPanel.ShowModal("Set Global Settings", "This will update all building records of this type to the current number of apartments across all saves!", (comp, ret) =>
            {
                if (ret != 1)
                    return;
                SetPrefabGlobalSettings(true);
            });
        }

        private void SetPrefabGlobalSettings(bool isGlobal)
        {
            ushort buildingID = WorldInfoPanel.GetCurrentInstanceID().Building;
            var buildingInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info;
            string BuildingAIstr = buildingInfo.GetAI().GetType().Name;
            var m_apartmentsNumTextfield = ApartmentNumberPanel.Find<UITextField>("ApartmentNumberTextField");

            if (!isGlobal)
            {
                // set new prefab settings according to the building current settings
                var newPrefabRecord = new HousingManager.PrefabRecord
                {
                    InfoName = buildingInfo.name,
                    BuildingAI = BuildingAIstr,
                    NumOfApartments = int.Parse(m_apartmentsNumTextfield.text)
                };

                // clear all individual building settings of this type
                var buildingsList = HousingManager.BuildingRecords.Where(item =>
                {
                    var Info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[item.Key].Info;
                    return Info.name == buildingInfo.name && Info.GetAI().GetType().Name == BuildingAIstr;
                }).ToList();

                foreach (var item in buildingsList)
                {
                    var buildingRecord = HousingManager.GetBuildingRecord(item.Key);
                    buildingRecord.IsDefault = false;
                    buildingRecord.IsPrefab = true;
                    buildingRecord.IsGlobal = false;
                    HousingManager.SetBuildingRecord(item.Key, buildingRecord);
                }

                if (HousingManager.PrefabExist(buildingInfo))
                {
                    // update the prefab
                    var prefabRecord = HousingManager.GetPrefab(buildingInfo);

                    prefabRecord.NumOfApartments = newPrefabRecord.NumOfApartments;

                    HousingManager.SetPrefab(prefabRecord);
                }
                else
                {
                    // create new prefab
                    HousingManager.CreatePrefab(newPrefabRecord);
                }
            }
            else
            {
                // set global settings

                // clear all individual building settings of this type
                var buildingsList = HousingManager.BuildingRecords.Where(item =>
                {
                    var Info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[item.Key].Info;
                    return Info.name == buildingInfo.name && Info.GetAI().GetType().Name == BuildingAIstr;
                }).ToList();

                foreach (var item in buildingsList)
                {
                    var buildingRecord = HousingManager.GetBuildingRecord(item.Key);
                    buildingRecord.IsDefault = false;
                    buildingRecord.IsPrefab = false;
                    buildingRecord.IsGlobal = true;
                    HousingManager.SetBuildingRecord(item.Key, buildingRecord);
                }

                // set new global settings according to the building current settings
                var newGlobalRecord = new Housing
                {
                    Name = buildingInfo.name,
                    BuildingAI = BuildingAIstr,
                    NumOfApartments = int.Parse(m_apartmentsNumTextfield.text)
                };

                // try get global settings and update them or create new global settings for this building type
                // if not exist and apply the settings to all the individual buildings
                var globalRecord = HousingConfig.Config.GetGlobalSettings(buildingInfo);

                if (globalRecord != null)
                {
                    globalRecord.NumOfApartments = newGlobalRecord.NumOfApartments;

                    HousingConfig.Config.SetGlobalSettings(globalRecord);
                }
                else
                {
                    HousingConfig.Config.CreateGlobalSettings(newGlobalRecord);
                }
            }
            RefreshData();
        }

        public void CreateOrEnsure(bool is_new, ushort buildingID, ref Building data, int numOfApartments, int workCount, int studentCount)
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

    }

}
