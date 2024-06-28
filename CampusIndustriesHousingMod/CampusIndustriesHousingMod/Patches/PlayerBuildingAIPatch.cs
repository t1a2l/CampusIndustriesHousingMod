using CampusIndustriesHousingMod.AI;
using HarmonyLib;

namespace CampusIndustriesHousingMod.Patches
{
    [HarmonyPatch(typeof(PlayerBuildingAI))]
	public static class PlayerBuildingAIPatch
    {
        [HarmonyPatch(typeof(PlayerBuildingAI), "SimulationStepActive")]
        [HarmonyPrefix]
        public static void Prefix(PlayerBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, ref byte __state)
        {
            if (buildingData.Info.GetAI() is DormsAI || buildingData.Info.GetAI() is BarracksAI)
            {
                __state = buildingData.m_citizenCount;
            } 
        }

        [HarmonyPatch(typeof(PlayerBuildingAI), "SimulationStepActive")]
        [HarmonyPostfix]
        public static void Postfix(PlayerBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, ref byte __state)
        {
            if (buildingData.Info.GetAI() is DormsAI || buildingData.Info.GetAI() is BarracksAI)
            {
                buildingData.m_citizenCount = __state;
            }
        }
    }
}
