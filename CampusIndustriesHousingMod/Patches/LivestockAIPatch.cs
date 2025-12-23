using CampusIndustriesHousingMod.AI;
using ColossalFramework;
using HarmonyLib;

namespace CampusIndustriesHousingMod.Patches
{
    [HarmonyPatch(typeof(LivestockAI))]
    public static class LivestockAIPatch
    {
        [HarmonyPatch(typeof(LivestockAI), "LoadInstance")]
        [HarmonyPrefix]
        public static bool LoadInstance(ushort instanceID, ref CitizenInstance data)
        {
            if(data.m_targetBuilding != 0)
            {
                var buildingAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[data.m_targetBuilding].Info.m_buildingAI;
                if (buildingAI is BarracksAI)
                {
                    Singleton<CitizenManager>.instance.ReleaseCitizenInstance(instanceID);
                    Singleton<CitizenManager>.instance.ReleaseCitizen(data.m_citizen);
                    return false;
                }
            }
            return true;
        }
    }
}
