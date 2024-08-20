using CampusIndustriesHousingMod.AI;
using CampusIndustriesHousingMod.Managers;
using HarmonyLib;

namespace CampusIndustriesHousingMod.Patches
{
    [HarmonyPatch(typeof(PlayerBuildingAI))]
	public static class PlayerBuildingAIPatch
    {
        [HarmonyPatch(typeof(PlayerBuildingAI), "CreateBuilding")]
        [HarmonyPrefix]
        public static void CreateBuildingPrefix(PlayerBuildingAI __instance, ushort buildingID, ref Building data)
        {
            if (data.Info.GetAI() is BarracksAI barracksAI)
            {
                var buildingRecord = HousingManager.CreateBuildingRecord(buildingID);
                barracksAI.numApartments = buildingRecord.NumOfApartments;
            }
            else if (data.Info.GetAI() is DormsAI dormsAI)
            {
                var buildingRecord = HousingManager.CreateBuildingRecord(buildingID);
                dormsAI.numApartments = buildingRecord.NumOfApartments;
            }
        }

        [HarmonyPatch(typeof(PlayerBuildingAI), "BuildingLoaded")]
        [HarmonyPrefix]
        public static void BuildingLoadedPrefix(PlayerBuildingAI __instance, ushort buildingID, ref Building data)
        {
            if(data.Info.GetAI() is BarracksAI || data.Info.GetAI() is DormsAI)
            {
                HousingManager.BuildingRecord buildingRecord;

                if (!HousingManager.BuildingRecordExist(buildingID))
                {
                    buildingRecord = HousingManager.CreateBuildingRecord(buildingID);
                }
                else
                {
                    buildingRecord = HousingManager.GetBuildingRecord(buildingID);
                }

                if (data.Info.GetAI() is BarracksAI barracksAI)
                {
                    barracksAI.numApartments = buildingRecord.NumOfApartments;
                }
                else if (data.Info.GetAI() is DormsAI dormsAI)
                {
                    dormsAI.numApartments = buildingRecord.NumOfApartments;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerBuildingAI), "SimulationStepActive")]
        [HarmonyPrefix]
        public static void SimulationStepActivePrefix(PlayerBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, ref byte __state)
        {
            if (buildingData.Info.GetAI() is DormsAI || buildingData.Info.GetAI() is BarracksAI)
            {
                __state = buildingData.m_citizenCount;
            } 
        }

        [HarmonyPatch(typeof(PlayerBuildingAI), "SimulationStepActive")]
        [HarmonyPostfix]
        public static void SimulationStepActivePostfix(PlayerBuildingAI __instance, ushort buildingID, ref Building buildingData, ref Building.Frame frameData, ref byte __state)
        {
            if (buildingData.Info.GetAI() is DormsAI || buildingData.Info.GetAI() is BarracksAI)
            {
                buildingData.m_citizenCount = __state;
            }
        }

        [HarmonyPatch(typeof(PlayerBuildingAI), "ReleaseBuilding")]
        [HarmonyPrefix]
        public static void ReleaseBuildingPrefix(PlayerBuildingAI __instance, ushort buildingID, ref Building data)
        {
            if (HousingManager.BuildingRecordExist(buildingID))
            {
                HousingManager.RemoveBuildingRecord(buildingID);
            }
        }
    }
}
