using ColossalFramework;
using HarmonyLib;

namespace CampusIndustriesHousingMod
{
    [HarmonyPatch(typeof(ResidentAI))]
    class ResidentAIPatch
    {

        [HarmonyPatch(typeof(ResidentAI), "CanMakeBabies")]
        [HarmonyPrefix]
        public static bool CanMakeBabies(uint citizenID, ref Citizen data, ref bool __result)
        {
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            Building homeBuilding = buildingManager.m_buildings.m_buffer[data.m_homeBuilding];
            if(homeBuilding.Info.m_buildingAI is DormsAI)
            {
                __result = false;
                return false;
            } 

            return true;
        }

        [HarmonyPatch(typeof(ResidentAI), "TryMoveAwayFromHome")]
        [HarmonyPrefix]
        public static bool TryMoveAwayFromHome(uint citizenID, ref Citizen data)
        {
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            Building homeBuilding = buildingManager.m_buildings.m_buffer[data.m_homeBuilding];
            if(homeBuilding.Info.m_buildingAI is DormsAI)
            {
                return false;
            } 

            return true;
        }

        [HarmonyPatch(typeof(ResidentAI), "TryFindPartner")]
        [HarmonyPrefix]
        public static bool TryFindPartner(uint citizenID, ref Citizen data)
        {
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            Building homeBuilding = buildingManager.m_buildings.m_buffer[data.m_homeBuilding];
            if(homeBuilding.Info.m_buildingAI is DormsAI)
            {
                return false;
            } 

            return true;
        }
    }
}
