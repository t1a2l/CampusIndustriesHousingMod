using ICities;
using ColossalFramework.UI;
using System.IO;
using System.Xml.Serialization;
using System;
using ColossalFramework;
using CampusIndustriesHousingMod.AI;

namespace CampusIndustriesHousingMod.Utils  
{
    public class OptionsManager {

        private static readonly string[] CAPACITY_LABELS = new string[] { "Give Em Room (x0.5)", "Realistic (x1.0)", "Just a bit More (x1.5)", "Gameplay over Realism (x2.0)", "Who needs Living Space? (x2.5)", "Pack em like Sardines! (x3.0)" };
        private static readonly float[] CAPACITY_VALUES = new float[] { 0.5f, 1.0f, 1.5f, 2.0f, 2.5f, 3.0f };

        private static readonly string[] BARRACKS_INCOME_LABELS = new string[] { "Communisim is Key (Full Maintenance)", "Workers Families can Help a Little (Half Maintenance at Full Capacity)", "Make the Workers Families Pay (No Maintenance at Full Capacity)", "Workers Barracks should be Profitable (Maintenance becomes Profit at Full Capacity)", "Twice the Pain, Twice the Gain (2x Maintenance, 2x Profit)", "Show me the Money! (Profit x2, Normal Maintenance)" };
        private static readonly string[] DORMS_INCOME_LABELS = new string[] { "Communisim is Key (Full Maintenance)", "Students can Help a Little (Half Maintenance at Full Capacity)", "Make the Students Pay (No Maintenance at Full Capacity)", "Students Dormitories should be Profitable (Maintenance becomes Profit at Full Capacity)", "Twice the Pain, Twice the Gain (2x Maintenance, 2x Profit)", "Show me the Money! (Profit x2, Normal Maintenance)" };

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

        public void initialize(UIHelperBase helper) 
        {
            Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.initialize -- Initializing Menu Options");
            UIHelperBase group = helper.AddGroup("Housing Global Settings");
            this.barracksCapacityDropDown = (UIDropDown) group.AddDropdown("Barracks Capacity Modifier", CAPACITY_LABELS, 1, handleCapacityChange);
            this.barracksIncomeDropDown = (UIDropDown) group.AddDropdown("Barracks Income Modifier", BARRACKS_INCOME_LABELS, 2, handleIncomeChange);
            group.AddSpace(2);
            this.dormsCapacityDropDown = (UIDropDown) group.AddDropdown("Dorms Capacity Modifier", CAPACITY_LABELS, 1, handleCapacityChange);
            this.dormsIncomeDropDown = (UIDropDown) group.AddDropdown("Dorms Income Modifier", DORMS_INCOME_LABELS, 2, handleIncomeChange);
            group.AddSpace(5);
            group.AddButton("Save", saveOptions);

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


        private void handleCapacityChange(int newSelection) 
        {
            // Do nothing until Save is pressed
        }

        private void handleIncomeChange(int newSelection) 
        {
            // Do nothing until Save is pressed
        }

        public void updateBarracksCapacity() 
        {
            this.updateBarracksCapacity(this.barracksCapacityModifier);
        }

        public void updateDormsCapacity() 
        {
            this.updateDormsCapacity(this.dormsCapacityModifier);
        }

        public float getBarracksCapacityModifier() 
        {
            return this.barracksCapacityModifier;
        }

        public float getDormsCapacityModifier() 
        {
            return this.dormsCapacityModifier;
        }

        public IncomeValues getBarracksIncomeModifier() 
        {
            return this.barracksIncomeValue;
        }

        public IncomeValues getDormsIncomeModifier() 
        {
            return this.dormsIncomeValue;
        }

        public void updateBarracksCapacity(float targetValue) 
        {

            Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.updateBarracksCapacity -- Updating barracks capacity with modifier: {0}", targetValue);
            for (uint index = 0; PrefabCollection<BuildingInfo>.LoadedCount() > index; ++index) {
                BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded(index);
                if (buildingInfo != null && buildingInfo.m_buildingAI is BarracksAI barracksAI) {
                    barracksAI.updateCapacity(targetValue);
                }
            }

            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            for (ushort i=0; i < buildingManager.m_buildings.m_buffer.Length; i++) {
                if (buildingManager.m_buildings.m_buffer[i].Info != null && buildingManager.m_buildings.m_buffer[i].Info.m_buildingAI != null && buildingManager.m_buildings.m_buffer[i].Info.m_buildingAI is BarracksAI barracksAI) {
                    barracksAI.validateCapacity(i, ref buildingManager.m_buildings.m_buffer[i], true);
                }
            }
        }

        public void updateDormsCapacity(float targetValue) 
        {

            Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.updateDormsCapacity -- Updating dorms capacity with modifier: {0}", targetValue);
            for (uint index = 0; PrefabCollection<BuildingInfo>.LoadedCount() > index; ++index) {
                BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded(index);
                if (buildingInfo != null && buildingInfo.m_buildingAI is DormsAI dormsAI) {
                    dormsAI.updateCapacity(targetValue);
                }
            }

            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            for (ushort i=0; i < buildingManager.m_buildings.m_buffer.Length; i++) {
                if (buildingManager.m_buildings.m_buffer[i].Info != null && buildingManager.m_buildings.m_buffer[i].Info.m_buildingAI != null && buildingManager.m_buildings.m_buffer[i].Info.m_buildingAI is DormsAI dormsAI) {
                    dormsAI.validateCapacity(i, ref buildingManager.m_buildings.m_buffer[i], true);
                }
            }
        }

        private void saveOptions() 
        {
            Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.saveOptions -- Saving Options");
            Options options = new()
            {
                barracksCapacityModifierSelectedIndex = -1,
                dormsCapacityModifierSelectedIndex = -1
            };

            if (this.barracksCapacityDropDown != null) 
            {
                int barracksCapacitySelectedIndex = this.barracksCapacityDropDown.selectedIndex;
                options.barracksCapacityModifierSelectedIndex = barracksCapacitySelectedIndex;
                if (barracksCapacitySelectedIndex >= 0) 
                {
                    Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.saveOptions -- Barracks Capacity Modifier Set to: {0}", CAPACITY_VALUES[barracksCapacitySelectedIndex]);
                    this.barracksCapacityModifier = CAPACITY_VALUES[barracksCapacitySelectedIndex];
                    this.updateBarracksCapacity(CAPACITY_VALUES[barracksCapacitySelectedIndex]);
                }
            }

            if (this.barracksIncomeDropDown != null) 
            {
                int barracksIncomeSelectedIndex = this.barracksIncomeDropDown.selectedIndex + 1;
                options.barracksIncomeModifierSelectedIndex = barracksIncomeSelectedIndex;
                if (barracksIncomeSelectedIndex >= 0) 
                {
                    Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.saveOptions -- Barracks Income Modifier Set to: {0}", (IncomeValues) barracksIncomeSelectedIndex);
                    this.barracksIncomeValue = (IncomeValues) barracksIncomeSelectedIndex;
                }
            }

            if(this.dormsCapacityDropDown != null) 
            {
                int dormsCapacitySelectedIndex = this.dormsCapacityDropDown.selectedIndex;
                options.dormsCapacityModifierSelectedIndex = dormsCapacitySelectedIndex;
                if (dormsCapacitySelectedIndex >= 0) 
                {
                    Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.saveOptions -- Dorms Capacity Modifier Set to: {0}", CAPACITY_VALUES[dormsCapacitySelectedIndex]);
                    this.dormsCapacityModifier = CAPACITY_VALUES[dormsCapacitySelectedIndex];
                    this.updateDormsCapacity(CAPACITY_VALUES[dormsCapacitySelectedIndex]);
                }
            }

            if (this.dormsIncomeDropDown != null) 
            {
                int dormsIncomeSelectedIndex = this.dormsIncomeDropDown.selectedIndex + 1;
                options.dormsIncomeModifierSelectedIndex = dormsIncomeSelectedIndex;
                if (dormsIncomeSelectedIndex >= 0) 
                {
                    Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.saveOptions -- Dorms Income Modifier Set to: {0}", (IncomeValues) dormsIncomeSelectedIndex);
                    this.dormsIncomeValue = (IncomeValues) dormsIncomeSelectedIndex;
                }
            }

            try 
            {
                using StreamWriter streamWriter = new("CampusIndustriesHousingModOptions.xml"); 
                new XmlSerializer(typeof(OptionsManager.Options)).Serialize(streamWriter, options);
            } 
            catch (Exception e) 
            {
                Logger.logError(Logger.LOG_OPTIONS, "Error saving options: {0} -- {1}", e.Message, e.StackTrace);
            }

        }

        public void loadOptions() 
        {
            Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.loadOptions -- Loading Options");
            Options options = new Options();

            try 
            {
                using StreamReader streamReader = new StreamReader("CampusIndustriesHousingModOptions.xml"); 
                options = (Options)new XmlSerializer(typeof(Options)).Deserialize(streamReader);
            } 
            catch (FileNotFoundException) 
            {
                // Options probably not serialized yet, just return
                return;
            } 
            catch (Exception e) 
            {
                Logger.logError(Logger.LOG_OPTIONS, "Error loading options: {0} -- {1}", e.Message, e.StackTrace);
                return;
            }

            if (options.barracksCapacityModifierSelectedIndex != -1) 
            {
                Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.loadOptions -- Loading Barracks Capacity Modifier to: x{0}", CAPACITY_VALUES[options.barracksCapacityModifierSelectedIndex]);
                this.barracksCapacityDropDown.selectedIndex = options.barracksCapacityModifierSelectedIndex;
                this.barracksCapacityModifier = CAPACITY_VALUES[options.barracksCapacityModifierSelectedIndex];
            }

            if (options.barracksIncomeModifierSelectedIndex > 0) 
            {
                Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.loadOptions -- Loading Barracks Income Modifier to: {0}", (IncomeValues) options.barracksIncomeModifierSelectedIndex);
                this.barracksIncomeDropDown.selectedIndex = options.barracksIncomeModifierSelectedIndex - 1;
                this.barracksIncomeValue = (IncomeValues) options.barracksIncomeModifierSelectedIndex;
            }

            if (options.dormsCapacityModifierSelectedIndex != -1) 
            {
                Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.loadOptions -- Loading Dorms Capacity Modifier to: x{0}", CAPACITY_VALUES[options.dormsCapacityModifierSelectedIndex]);
                this.dormsCapacityDropDown.selectedIndex = options.dormsCapacityModifierSelectedIndex;
                this.dormsCapacityModifier = CAPACITY_VALUES[options.dormsCapacityModifierSelectedIndex];
            }

            if (options.dormsIncomeModifierSelectedIndex > 0) 
            {
                Logger.logInfo(Logger.LOG_OPTIONS, "OptionsManager.loadOptions -- Loading Dorms Income Modifier to: {0}", (IncomeValues) options.dormsIncomeModifierSelectedIndex);
                this.dormsIncomeDropDown.selectedIndex = options.dormsIncomeModifierSelectedIndex - 1;
                this.dormsIncomeValue = (IncomeValues) options.dormsIncomeModifierSelectedIndex;
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
