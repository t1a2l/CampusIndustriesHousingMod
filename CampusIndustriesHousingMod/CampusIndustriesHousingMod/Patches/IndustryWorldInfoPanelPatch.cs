using CampusIndustriesHousingMod.AI;
using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using System;
using System.Reflection;

namespace CampusIndustriesHousingMod.Patches
{
    [HarmonyPatch(typeof(IndustryWorldInfoPanel))]
    public static class IndustryWorldInfoPanelPatch
    {
        [HarmonyPatch(typeof(IndustryWorldInfoPanel), "UpdateWorkersAndTotalUpkeep")]
        [HarmonyPostfix]
        public static void UpdateWorkersAndTotalUpkeep(IndustryWorldInfoPanel __instance)
        {
            var m_InstanceID = (InstanceID)typeof(WorldInfoPanel).GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            if(m_InstanceID.Park == 0)
            {
                Logger.LogInfo(Logger.LOG_INDUSTRY, "Industry m_InstanceID Park is 0");
                return;
            }

            BuildingManager buildingManager = Singleton<BuildingManager>.instance;

            var industry_buildings = GetIndustryBuildings(m_InstanceID.Park);

            var total_ocuppied_apartment_num = 0;
            var total_apartment_num = 0;

            Logger.LogInfo(Logger.LOG_INDUSTRY, "Industry Buildings number: {0}", industry_buildings.m_size);

            for (ushort i = 0; i < industry_buildings.m_size; i++) 
            {
                var buildingId = industry_buildings[i];
                Logger.LogInfo(Logger.LOG_INDUSTRY, "Industry Building id: {0}", buildingId);
                if(buildingId == 0)
                {
                    continue;
                }
                Building industryBuilding = buildingManager.m_buildings.m_buffer[buildingId];
                if(industryBuilding.Info.GetAI() is BarracksAI barracksAI)
                {
                    barracksAI.getOccupancyDetails(ref industryBuilding, out int numResidents, out int numApartmentsOccupied);
                    total_ocuppied_apartment_num += numApartmentsOccupied;
                    total_apartment_num += barracksAI.getModifiedCapacity(buildingId, ref industryBuilding);
                    Logger.LogInfo(Logger.LOG_INDUSTRY, "Industry Building occupied apartments: {0}", numApartmentsOccupied);
                }
            }

            var barracks_capacity = StringUtils.SafeFormat("{0} / {1} barracks capacity", total_ocuppied_apartment_num, total_apartment_num);

            Logger.LogInfo(Logger.LOG_INDUSTRY, "Barracks Apartment Capacity: {0} / {1}", total_ocuppied_apartment_num, total_apartment_num);

            var m_workersInfoLabel = (UILabel)typeof(IndustryWorldInfoPanel).GetField("m_workersInfoLabel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            Logger.LogInfo(Logger.LOG_INDUSTRY, "old Industry worker label: {0}", m_workersInfoLabel);

            m_workersInfoLabel.text = m_workersInfoLabel.text + Environment.NewLine + barracks_capacity + Environment.NewLine;

            Logger.LogInfo(Logger.LOG_INDUSTRY, "new Industry worker label: {0}", m_workersInfoLabel);

        }

        private static FastList<ushort> GetIndustryBuildings(byte industry)
        {
            var buildings = new FastList<ushort>();
            FastList<ushort> serviceBuildings = Singleton<BuildingManager>.instance.GetServiceBuildings(ItemClass.Service.PlayerIndustry);
		    for (int i = 0; i < serviceBuildings.m_size; i++)
		    {
			    IndustryBuildingAI industryBuildingAI = Singleton<BuildingManager>.instance.m_buildings.m_buffer[serviceBuildings[i]].Info.m_buildingAI as IndustryBuildingAI;
			    if (industryBuildingAI != null && GetArea(ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[serviceBuildings[i]]) == industry)
			    {
				    buildings.Add(serviceBuildings[i]);
			    }
		    }
            return buildings;
        }

        private static byte GetArea(ref Building data)
		{
			DistrictManager instance = Singleton<DistrictManager>.instance;
			byte b = instance.GetPark(data.m_position);
			IndustryBuildingAI industryBuildingAI = data.Info.m_buildingAI as IndustryBuildingAI;
			if (b != 0)
			{
				if (!instance.m_parks.m_buffer[b].IsIndustry)
				{
					b = 0;
				}
				else if (industryBuildingAI != null && (industryBuildingAI.m_industryType == DistrictPark.ParkType.Industry || industryBuildingAI.m_industryType != instance.m_parks.m_buffer[b].m_parkType))
				{
					b = 0;
				}
			}
			return b;
		}

    }
}
