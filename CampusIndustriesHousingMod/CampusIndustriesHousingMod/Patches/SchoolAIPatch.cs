using CampusIndustriesHousingMod.AI;
using HarmonyLib;
using System.Reflection;

namespace CampusIndustriesHousingMod.Patches
{
	[HarmonyPatch(typeof(SchoolAI))]
	public static class SchoolAIPatch
	{
		private delegate void PlayerBuildingAICreateBuildingDelegate(PlayerBuildingAI __instance, ushort buildingID, ref Building data);
        private static PlayerBuildingAICreateBuildingDelegate BaseCreateBuilding = AccessTools.MethodDelegate<PlayerBuildingAICreateBuildingDelegate>(typeof(PlayerBuildingAI).GetMethod("CreateBuilding", BindingFlags.Instance | BindingFlags.Public), null, false);

		private delegate void PlayerBuildingAIBuildingLoadedDelegate(PlayerBuildingAI __instance, ushort buildingID, ref Building data, uint version);
        private static PlayerBuildingAIBuildingLoadedDelegate BaseBuildingLoaded = AccessTools.MethodDelegate<PlayerBuildingAIBuildingLoadedDelegate>(typeof(PlayerBuildingAI).GetMethod("BuildingLoaded", BindingFlags.Instance | BindingFlags.Public), null, false);

		private delegate void PlayerBuildingAIEndRelocatingDelegate(PlayerBuildingAI __instance, ushort buildingID, ref Building data);
        private static PlayerBuildingAIEndRelocatingDelegate BaseEndRelocating = AccessTools.MethodDelegate<PlayerBuildingAIEndRelocatingDelegate>(typeof(PlayerBuildingAI).GetMethod("EndRelocating", BindingFlags.Instance | BindingFlags.Public), null, false);

		[HarmonyPatch(typeof(SchoolAI), "CreateBuilding")]
        [HarmonyPrefix]
        public static bool CreateBuilding(SchoolAI __instance, ushort buildingID, ref Building data)
        {
			if(data.Info.GetAI() is DormsAI)
			{
				BaseCreateBuilding(__instance, buildingID, ref data);
				return false;
			}
            return true;
        }

		[HarmonyPatch(typeof(SchoolAI), "BuildingLoaded")]
        [HarmonyPrefix]
		public static bool BuildingLoaded(SchoolAI __instance, ushort buildingID, ref Building data, uint version)
		{
			if(data.Info.GetAI() is DormsAI)
			{
				BaseBuildingLoaded(__instance, buildingID, ref data, version);
				return false;
			}
			return true;
		}

		[HarmonyPatch(typeof(SchoolAI), "EndRelocating")]
        [HarmonyPrefix]
		public static bool EndRelocating(SchoolAI __instance, ushort buildingID, ref Building data)
		{
			if(data.Info.GetAI() is DormsAI)
			{
				BaseEndRelocating(__instance, buildingID, ref data);
				return false;
			}
			return true;
		}
	}
}
