using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using System;
using System.Reflection;

namespace CampusIndustriesHousingMod
{
    [HarmonyPatch(typeof(CampusWorldInfoPanel))]
    class CampusWorldInfoPanelPatch
    {
        [HarmonyPatch(typeof(CampusWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        public static void UpdateBindings(CampusWorldInfoPanel __instance)
        {
            var m_InstanceID = (InstanceID)typeof(WorldInfoPanel).GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            DistrictManager instance = Singleton<DistrictManager>.instance;
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            var total_ocuppied_apartment_num = 0;
            var total_apartment_num = 0;

            for (uint i=0; instance.m_parks.m_buffer[m_InstanceID.Park].m_buildings.m_buffer.Length > i; ++i) {
                var buildingId = instance.m_parks.m_buffer[m_InstanceID.Park].m_buildings.m_buffer[i];
                Building campusBuilding = buildingManager.m_buildings.m_buffer[buildingId];
                if(campusBuilding.Info.GetAI() is DormsAI dormsAI)
                {
                    dormsAI.getOccupancyDetails(ref campusBuilding, out int numResidents, out int numApartmentsOccupied);
                    total_ocuppied_apartment_num += numApartmentsOccupied;
                    total_apartment_num += dormsAI.getModifiedCapacity();
                }
            }

            var dorms_capacity = StringUtils.SafeFormat("Dorms Apartment Capacity: {0} / {1}", total_ocuppied_apartment_num, total_apartment_num);

            var m_studentCapacityLabel = (UILabel)typeof(CampusWorldInfoPanel).GetField("m_studentCapacityLabel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            m_studentCapacityLabel.text = m_studentCapacityLabel.text + Environment.NewLine + dorms_capacity;

        }

    }
}
