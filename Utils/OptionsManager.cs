using ICities;
using ColossalFramework.UI;
using System.IO;
using System.Xml.Serialization;
using System;
using CampusIndustriesHousingMod.Managers;

namespace CampusIndustriesHousingMod.Utils
{
    public class OptionsManager
    {
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

        private UIDropDown barracksIncomeDropDown;
        private UIDropDown dormsIncomeDropDown;
        private IncomeValues barracksIncomeValue = IncomeValues.NO_MAINTENANCE;
        private IncomeValues dormsIncomeValue = IncomeValues.NO_MAINTENANCE;

        public void Initialize(UIHelperBase helper)
        {
            Logger.LogInfo(Logger.LOG_OPTIONS, "OptionsManager.Initialize -- Initializing Menu Options");
            UIHelperBase group = helper.AddGroup("Housing Global Settings");
            barracksIncomeDropDown = (UIDropDown)group.AddDropdown("Barracks Income Modifier", BARRACKS_INCOME_LABELS, 2, HandleIncomeChange);
            group.AddSpace(2);
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

        private void HandleIncomeChange(int newSelection)
        {
            // Do nothing until Save is pressed
        }

        public IncomeValues GetBarracksIncomeModifier()
        {
            return barracksIncomeValue;
        }

        public IncomeValues GetDormsIncomeModifier()
        {
            return dormsIncomeValue;
        }

        private void SaveOptions()
        {
            Logger.LogInfo(Logger.LOG_OPTIONS, "OptionsManager.SaveOptions -- Saving Options");
            Options options = new();

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
                new XmlSerializer(typeof(Options)).Serialize(streamWriter, options);
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

            if (options.barracksIncomeModifierSelectedIndex > 0)
            {
                Logger.LogInfo(Logger.LOG_OPTIONS, "OptionsManager.LoadOptions -- Loading Barracks Income Modifier to: {0}", (IncomeValues)options.barracksIncomeModifierSelectedIndex);
                barracksIncomeDropDown.selectedIndex = options.barracksIncomeModifierSelectedIndex - 1;
                barracksIncomeValue = (IncomeValues)options.barracksIncomeModifierSelectedIndex;
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
            public int barracksIncomeModifierSelectedIndex;
            public int dormsIncomeModifierSelectedIndex;
        }
    }
}
