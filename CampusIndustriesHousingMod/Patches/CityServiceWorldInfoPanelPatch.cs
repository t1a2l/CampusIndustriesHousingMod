using CampusIndustriesHousingMod.UI;
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
            if (cityServiceHousingUIPanel == null)
            {
                cityServiceHousingUIPanel = new HousingUIPanel(m_cityServiceWorldInfoPanel, buttonPanels);
            }
        }
    }
}
