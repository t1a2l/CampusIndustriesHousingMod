using HarmonyLib;

namespace CampusIndustriesHousingMod.Patches
{
    [HarmonyPatch(typeof(CityServiceWorldInfoPanel))]
	public static class CityServiceWorldInfoPanelPatch
	{
		[HarmonyPatch(typeof(CampusWorldInfoPanel), "OnSetTarget")]
        [HarmonyPostfix]
        public static void OnSetTarget_Postfix(CityServiceWorldInfoPanel __instance)
        {
            CampusIndustriesHousingMod.Panel?.RefreshData();
        }
	}
}
