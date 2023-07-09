using CampusIndustriesHousingMod.UI;
using HarmonyLib;

namespace CampusIndustriesHousingMod.Patches
{
    [HarmonyPatch(typeof(CityServiceWorldInfoPanel))]
	public static class CityServiceWorldInfoPanelPatch
	{
		[HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnSetTarget")]
        [HarmonyPostfix]
        public static void OnSetTarget_Postfix(CityServiceWorldInfoPanel __instance)
        {
            if(HousingUIPanel.m_uiMainPanel == null)
            {
                HousingUIPanel.Init();
            }
            HousingUIPanel.RefreshData();
        }
	}
}
