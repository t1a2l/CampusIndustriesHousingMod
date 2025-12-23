using CampusIndustriesHousingMod.AI;
using HarmonyLib;

namespace CampusIndustriesHousingMod.Patches
{
    [HarmonyPatch(typeof(BuildingAI))]
    public static class BuildingAIPatch
    {
        [HarmonyPatch(typeof(BuildingAI), "SpawnAnimals")]
        [HarmonyPrefix]
        public static bool SpawnAnimals(ushort buildingID, ref Building data)
        {
            if (data.Info.GetAI() is BarracksAI)
            {
                return false;
            }
            return true;
        }
    }
}
