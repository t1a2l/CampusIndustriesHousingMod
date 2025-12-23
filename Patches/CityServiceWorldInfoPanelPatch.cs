using CampusIndustriesHousingMod.AI;
using CampusIndustriesHousingMod.UI;
using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using UnityEngine;

namespace CampusIndustriesHousingMod.Patches
{
    [HarmonyPatch(typeof(CityServiceWorldInfoPanel))]
	public static class CityServiceWorldInfoPanelPatch
	{
        private static HousingUIPanel cityServiceHousingUIPanel;

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnSetTarget")]
        [HarmonyPostfix]
        public static void OnSetTarget()
        {
            if (cityServiceHousingUIPanel == null)
            {
                CityServiceCreateUI();
            }
            cityServiceHousingUIPanel.UpdateBuildingData();
        }

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        public static void UpdateBindings(ref InstanceID ___m_InstanceID, ref UICheckBox ___m_OnOff)
        {
            if (___m_InstanceID.Type != InstanceType.Building || ___m_InstanceID.Building == 0)
            {
                return;
            }
            ushort building = ___m_InstanceID.Building;
            Building data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];
            if ((data.Info.GetAI() is BarracksAI || data.Info.GetAI() is DormsAI) && ___m_OnOff.isChecked)
            {
                ___m_OnOff.Hide();
            }
            else
            {
                ___m_OnOff.Show();
            }
        }

        private static void CityServiceCreateUI()
        {
            var m_cityServiceWorldInfoPanel = GameObject.Find("(Library) CityServiceWorldInfoPanel").GetComponent<CityServiceWorldInfoPanel>();
            var wrapper = m_cityServiceWorldInfoPanel?.Find("Wrapper");
            var mainSectionPanel = wrapper?.Find("MainSectionPanel");
            var mainBottom = mainSectionPanel?.Find("MainBottom");
            var buttonPanels = mainBottom?.Find("ButtonPanels").GetComponent<UIPanel>();
            if (buttonPanels == null)
            {
                return;
            }
            cityServiceHousingUIPanel ??= new HousingUIPanel(m_cityServiceWorldInfoPanel, buttonPanels);
        }
    }
}
