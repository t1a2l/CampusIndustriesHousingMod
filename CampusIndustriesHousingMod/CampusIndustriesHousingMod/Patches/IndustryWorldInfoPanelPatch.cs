using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using System;
using System.Reflection;

namespace CampusIndustriesHousingMod
{
    [HarmonyPatch(typeof(IndustryWorldInfoPanel))]
    class IndustryWorldInfoPanelPatch
    {
        [HarmonyPatch(typeof(IndustryWorldInfoPanel), "UpdateWorkersAndTotalUpkeep")]
        [HarmonyPostfix]
        public static void UpdateWorkersAndTotalUpkeep(IndustryWorldInfoPanel __instance)
        {
            var m_InstanceID = (InstanceID)typeof(WorldInfoPanel).GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            DistrictManager instance = Singleton<DistrictManager>.instance;
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            var total_ocuppied_apartment_num = 0;
            var total_apartment_num = 0;

            for (uint i=0; instance.m_parks.m_buffer[m_InstanceID.Park].m_buildings.m_buffer.Length > i; ++i) {
                var buildingId = instance.m_parks.m_buffer[m_InstanceID.Park].m_buildings.m_buffer[i];
                Building industryBuilding = buildingManager.m_buildings.m_buffer[buildingId];
                if(industryBuilding.Info.GetAI() is BarracksAI barracksAI)
                {
                    barracksAI.getOccupancyDetails(ref industryBuilding, out int numResidents, out int numApartmentsOccupied);
                    total_ocuppied_apartment_num += numApartmentsOccupied;
                    total_apartment_num += barracksAI.getModifiedCapacity();
                }
            }

            var barracks_capacity = StringUtils.SafeFormat("Barracks Apartment Capacity: {0} / {1}", total_ocuppied_apartment_num, total_apartment_num);

            var m_workersInfoLabel = (UILabel)typeof(IndustryWorldInfoPanel).GetField("m_workersInfoLabel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            m_workersInfoLabel.text = m_workersInfoLabel.text + Environment.NewLine + barracks_capacity;

        }

    }
}
