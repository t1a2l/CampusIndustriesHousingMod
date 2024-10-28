using ICities;
using ColossalFramework.UI;
using System.IO;
using System.Xml.Serialization;
using System;
using ColossalFramework;
using CampusIndustriesHousingMod.AI;
using CampusIndustriesHousingMod.Managers;

namespace CampusIndustriesHousingMod.Utils
{
    public class OptionsManager
    {
        private static readonly string[] CAPACITY_LABELS = ["Give Em Room (x0.5)", "Realistic (x1.0)", "Just a bit More (x1.5)", "Gameplay over Realism (x2.0)", "Who needs Living Space? (x2.5)", "Pack em like Sardines! (x3.0)"];
        private static readonly float[] CAPACITY_VALUES = [0.5f, 1.0f, 1.5f, 2.0f, 2.5f, 3.0f];

        private static readonly string[] BARRACKS_INCOME_LABELS = ["Communisim is Key (Full Maintenance)", "Workers Families can Help a Little (Half Maintenance at Full Capacity)", "Make the Workers Families Pay (No Maintenance at Full Capacity)", "Workers Barracks should be Profitable (Maintenance becomes Profit at Full Capacity)", "Twice the Pain, Twice the Gain (2x Maintenance, 2x Profit)", "Show me the Money! (Profit x2, Normal Maintenance)"];
        private static readonly string[] DORMS_INCOME_LABELS = ["Communisim is Key (Full Maintenance)", "Students can Help a Little (Half Maintenance at Full Capacity)", "Make the Students Pay (No Maintenance at Full Capacity)", "Students Dormitories should be Profitable (Maintenance becomes Profit at Full Capacity)", "Twice the Pain, Twice the Gain (2x Maintenance, 2x Profit)", "Show me the Money! (Profit x2, Normal Maintenance)"];

        public enum IncomeValues
        {
            FULL_MAINTENANCE = 1,
            HALF_MAINTENANCE = 2,
            NO_MAINTENANCE = 3,
            NORMAL_PROFIT = 4,
            DOUBLE_DOUBLE = 5,
            DOUBLE_PROFIT = 6
        };

        private UIDropDown barracksCapacityDropDown;
        private UIDropDown dormsCapacityDropDown;
        private float barracksCapacityModifier = -1.0f;
        private float dormsCapacityModifier = -1.0f;

        private UIDropDown barracksIncomeDropDown;
        private UIDropDown dormsIncomeDropDown;
        private IncomeValues barracksIncomeValue = IncomeValues.NO_MAINTENANCE;
        private IncomeValues dormsIncomeValue = IncomeValues.NO_MAINTENANCE;

        public void Initialize(UIHelperBase helper)
        {
            Logger.LogInfo(Logger.LOG_OPTIONS, "OptionsManager.Initialize -- Initializing Menu Options");
            UIHelperBase group = helper.AddGroup("Housing Global Settings");
            barracksCapacityDropDown = (UIDropDown)group.AddDropdown("Barracks Capacity Modifier", CAPACITY_LABELS, 1, HandleCapacityChange);
            barracksIncomeDropDown = (UIDropDown)group.AddDropdown("Barracks Income Modifier", BARRACKS_INCOME_LABELS, 2, HandleIncomeChange);
            group.AddSpace(2);
            dormsCapacityDropDown = (UIDropDown)group.AddDropdown("Dorms Capacity Modifier", CAPACITY_LABELS, 1, HandleCapacityChange);
            dormsIncomeDropDown = (UIDropDown)group.AddDropdown("Dorms Income Modifier", DORMS_INCOME_LABELS, 2, HandleIncomeChange);
            group.AddSpace(5);
            group.AddButton("Save", SaveOptions);

            UIHelperBase group_clear = helper.AddGroup("Housing Clear Settings, Use with Caution!! Can't be undone!");
            group_clear.AddButton("Clear All Buildings Records", ConfimDeleteBuildignRecords);
            group_clear.AddSpace(1);
            group_clear.AddButton("Clear All Buildings Prefab Records", ConfimDeletePrefabRecords);
            group_clear.AddSpace(1);
            group_clear.AddButton("Clear Housing Global Settings", ConfimDeleteGlobalConfig);
        }

        private void ConfimDeleteBuildignRecords()
        {
            ConfirmPanel.ShowModal("Delete All Building Records", "This will clear all building records!", (comp, ret) =>
            {
                if (ret != 1)
                    return;
                HousingManager.ClearBuildingRecords();
            });
        }

        private void ConfimDeletePrefabRecords()
        {
            ConfirmPanel.ShowModal("Delete All Prefab Records", "This will clear all prefab records!", (comp, ret) =>
            {
                if (ret != 1)
                    return;
                HousingManager.ClearPrefabRecords();
            });
        }

        private void ConfimDeleteGlobalConfig()
        {
            ConfirmPanel.ShowModal("Delete All Global Settings", "This will clear all global settings!", (comp, ret) =>
            {
                if (ret != 1)
                    return;
                HousingConfig.Config.ClearGlobalSettings();
            });
        }


        private void HandleCapacityChange(int newSelection)
        {
            // Do nothing until Save is pressed
        }

        private void HandleIncomeChange(int newSelection)
        {
            // Do nothing until Save is pressed
        }

        public void UpdateBarracksCapacity()
        {
            UpdateBarracksCapacity(barracksCapacityModifier);
        }

        public void UpdateDormsCapacity()
        {
            UpdateDormsCapacity(dormsCapacityModifier);
        }

        public float GetBarracksCapacityModifier()
        {
            return barracksCapacityModifier;
        }

        public float GetDormsCapacityModifier()
        {
            return dormsCapacityModifier;
        }

        public IncomeValues GetBarracksIncomeModifier()
        {
            return barracksIncomeValue;
        }

        public IncomeValues GetDormsIncomeModifier()
        {
            return dormsIncomeValue;
        }

        public void UpdateBarracksCapacity(float targetValue)
        {

            Logger.LogInfo(Logger.LOG_OPTIONS, "OptionsManager.UpdateBarracksCapacity -- Updating barracks capacity with modifier: {0}", targetValue);
            for (uint index = 0; PrefabCollection<BuildingInfo>.LoadedCount() > index; ++index)
            {
                BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded(index);
                if (buildingInfo != null && buildingInfo.m_buildingAI is BarracksAI barracksAI)
                {
                    barracksAI.UpdateCapacityModifier(targetValue);
                }
            }

            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            for (ushort i = 0; i < buildingManager.m_buildings.m_buffer.Length; i++)
            {
                if (buildingManager.m_buildings.m_buffer[i].Info != null && buildingManager.m_buildings.m_buffer[i].Info.m_buildingAI != null && buildingManager.m_buildings.m_buffer[i].Info.m_buildingAI is BarracksAI barracksAI)
                {
                    barracksAI.ValidateCapacity(i, ref buildingManager.m_buildings.m_buffer[i], true);
                }
            }
        }

        public void UpdateDormsCapacity(float targetValue)
        {

            Logger.LogInfo(Logger.LOG_OPTIONS, "OptionsManager.UpdateDormsCapacity -- Updating dorms capacity with modifier: {0}", targetValue);
            for (uint index = 0; PrefabCollection<BuildingInfo>.LoadedCount() > index; ++index)
            {
                BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded(index);
                if (buildingInfo != null && buildingInfo.m_buildingAI is DormsAI dormsAI)
                {
                    dormsAI.UpdateCapacityModifier(targetValue);
                }
            }

            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            for (ushort i = 0; i < buildingManager.m_buildings.m_buffer.Length; i++)
            {
                if (buildingManager.m_buildings.m_buffer[i].Info != null && buildingManager.m_buildings.m_buffer[i].Info.m_buildingAI != null && buildingManager.m_buildings.m_buffer[i].Info.m_buildingAI is DormsAI dormsAI)
                {
                    dormsAI.ValidateCapacity(i, ref buildingManager.m_buildings.m_buffer[i], true);
                }
            }
        }

        private void SaveOptions()
        {
            Logger.LogInfo(Logger.LOG_OPTIONS, "OptionsManager.SaveOptions -- Saving Options");
            Options options = new()
            {
                barracksCapacityModifierSelectedIndex = -1,
                dormsCapacityModifierSelectedIndex = -1
            };

            if (barracksCapacityDropDown != null)
            {
                int barracksCapacitySelectedIndex = barracksCapacityDropDown.selectedIndex;
                options.barracksCapacityModifierSelectedIndex = barracksCapacitySelectedIndex;
                if (barracksCapacitySelectedIndex >= 0)
                {
                    Logger.LogInfo(Logger.LOG_OPTIONS, "OptionsManager.SaveOptions -- Barracks Capacity Modifier Set to: {0}", CAPACITY_VALUES[barracksCapacitySelectedIndex]);
                    barracksCapacityModifier = CAPACITY_VALUES[barracksCapacitySelectedIndex];
                    UpdateBarracksCapacity(CAPACITY_VALUES[barracksCapacitySelectedIndex]);
                }
            }

            if (barracksIncomeDropDown != null)
            {
                int barracksIncomeSelectedIndex = barracksIncomeDropDown.selectedIndex + 1;
                options.barracksIncomeModifierSelectedIndex = barracksIncomeSelectedIndex;
                if (barracksIncomeSelectedIndex >= 0)
                {
                    Logger.LogInfo(Logger.LOG_OPTIONS, "OptionsManager.SaveOptions -- Barracks Income Modifier Set to: {0}", (IncomeValues)barracksIncomeSelectedIndex);
                    barracksIncomeValue = (IncomeValues)barracksIncomeSelectedIndex;
                }
            }

            if (dormsCapacityDropDown != null)
            {
                int dormsCapacitySelectedIndex = dormsCapacityDropDown.selectedIndex;
                options.dormsCapacityModifierSelectedIndex = dormsCapacitySelectedIndex;
                if (dormsCapacitySelectedIndex >= 0)
                {
                    Logger.LogInfo(Logger.LOG_OPTIONS, "OptionsManager.saveOptions -- Dorms Capacity Modifier Set to: {0}", CAPACITY_VALUES[dormsCapacitySelectedIndex]);
                    dormsCapacityModifier = CAPACITY_VALUES[dormsCapacitySelectedIndex];
                    UpdateDormsCapacity(CAPACITY_VALUES[dormsCapacitySelectedIndex]);
                }
            }

            if (dormsIncomeDropDown != null)
            {
                int dormsIncomeSelectedIndex = dormsIncomeDropDown.selectedIndex + 1;
                options.dormsIncomeModifierSelectedIndex = dormsIncomeSelectedIndex;
                if (dormsIncomeSelectedIndex >= 0)
                {
                    Logger.LogInfo(Logger.LOG_OPTIONS, "OptionsManager.saveOptions -- Dorms Income Modifier Set to: {0}", (IncomeValues)dormsIncomeSelectedIndex);
                    dormsIncomeValue = (IncomeValues)dormsIncomeSelectedIndex;
                }
            }

            try
            {
                using StreamWriter streamWriter = new("CampusIndustriesHousingModOptions.xml");
                new XmlSerializer(typeof(OptionsManager.Options)).Serialize(streamWriter, options);
            }
            catch (Exception e)
            {
                Logger.LogError(Logger.LOG_OPTIONS, "Error saving options: {0} -- {1}", e.Message, e.StackTrace);
            }

        }

        public void LoadOptions()
        {
            Logger.LogInfo(Logger.LOG_OPTIONS, "OptionsManager.LoadOptions -- Loading Options");
            Options options = new();

            try
            {
                using StreamReader streamReader = new("CampusIndustriesHousingModOptions.xml");
                options = (Options)new XmlSerializer(typeof(Options)).Deserialize(streamReader);
            }
            catch (FileNotFoundException)
            {
                // Options probably not serialized yet, just return
                return;
            }
            catch (Exception e)
            {
                Logger.LogError(Logger.LOG_OPTIONS, "Error loading options: {0} -- {1}", e.Message, e.StackTrace);
                return;
            }

            if (options.barracksCapacityModifierSelectedIndex != -1)
            {
                Logger.LogInfo(Logger.LOG_OPTIONS, "OptionsManager.LoadOptions -- Loading Barracks Capacity Modifier to: x{0}", CAPACITY_VALUES[options.barracksCapacityModifierSelectedIndex]);
                barracksCapacityDropDown.selectedIndex = options.barracksCapacityModifierSelectedIndex;
                barracksCapacityModifier = CAPACITY_VALUES[options.barracksCapacityModifierSelectedIndex];
            }

            if (options.barracksIncomeModifierSelectedIndex > 0)
            {
                Logger.LogInfo(Logger.LOG_OPTIONS, "OptionsManager.LoadOptions -- Loading Barracks Income Modifier to: {0}", (IncomeValues)options.barracksIncomeModifierSelectedIndex);
                barracksIncomeDropDown.selectedIndex = options.barracksIncomeModifierSelectedIndex - 1;
                barracksIncomeValue = (IncomeValues)options.barracksIncomeModifierSelectedIndex;
            }

            if (options.dormsCapacityModifierSelectedIndex != -1)
            {
                Logger.LogInfo(Logger.LOG_OPTIONS, "OptionsManager.LoadOptions -- Loading Dorms Capacity Modifier to: x{0}", CAPACITY_VALUES[options.dormsCapacityModifierSelectedIndex]);
                dormsCapacityDropDown.selectedIndex = options.dormsCapacityModifierSelectedIndex;
                dormsCapacityModifier = CAPACITY_VALUES[options.dormsCapacityModifierSelectedIndex];
            }

            if (options.dormsIncomeModifierSelectedIndex > 0)
            {
                Logger.LogInfo(Logger.LOG_OPTIONS, "OptionsManager.LoadOptions -- Loading Dorms Income Modifier to: {0}", (IncomeValues)options.dormsIncomeModifierSelectedIndex);
                dormsIncomeDropDown.selectedIndex = options.dormsIncomeModifierSelectedIndex - 1;
                dormsIncomeValue = (IncomeValues)options.dormsIncomeModifierSelectedIndex;
            }
        }

        public struct Options
        {
            public int barracksCapacityModifierSelectedIndex;
            public int barracksIncomeModifierSelectedIndex;
            public int dormsCapacityModifierSelectedIndex;
            public int dormsIncomeModifierSelectedIndex;
        }
    }
}
