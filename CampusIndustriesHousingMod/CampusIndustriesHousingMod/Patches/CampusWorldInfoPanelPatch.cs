using CampusIndustriesHousingMod.AI;
using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using System;
using System.Reflection;

namespace CampusIndustriesHousingMod.Patches
{
    [HarmonyPatch(typeof(CampusWorldInfoPanel))]
    public static class CampusWorldInfoPanelPatch
    {
        [HarmonyPatch(typeof(CampusWorldInfoPanel), "UpdateBindings")]
        [HarmonyPostfix]
        public static void UpdateBindings(CampusWorldInfoPanel __instance)
        {
            var m_InstanceID = (InstanceID)typeof(WorldInfoPanel).GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            if(m_InstanceID.Park == 0)
            {
                Logger.LogInfo(Logger.LOG_CAMPUS, "Campus m_InstanceID Park is 0");
                return;
            }

            DistrictManager instance = Singleton<DistrictManager>.instance;
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;

            var campus_buildings = instance.m_parks.m_buffer[m_InstanceID.Park].GetCampusBuildings(m_InstanceID.Park);

            var total_ocuppied_apartment_num = 0;
            var total_apartment_num = 0;

            Logger.LogInfo(Logger.LOG_CAMPUS, "Campus Buildings number: {0}", campus_buildings.m_size);

            for (ushort i = 0; i < campus_buildings.m_size; i++) 
            {
                var buildingId = campus_buildings[i];
                Logger.LogInfo(Logger.LOG_CAMPUS, "Campus Building id: {0}", buildingId);
                if(buildingId == 0)
                {
                    continue;
                }
                Building campusBuilding = buildingManager.m_buildings.m_buffer[buildingId];
                if(campusBuilding.Info.GetAI() is DormsAI dormsAI)
                {
                    dormsAI.getOccupancyDetails(ref campusBuilding, out _, out int numApartmentsOccupied);
                    total_ocuppied_apartment_num += numApartmentsOccupied;
                    total_apartment_num += dormsAI.getModifiedCapacity(buildingId, ref campusBuilding);
                    Logger.LogInfo(Logger.LOG_CAMPUS, "Campus Building occupied apartments: {0}", numApartmentsOccupied);
                }
            }

            var dorms_capacity = StringUtils.SafeFormat("Dorms Capacity: {0} / {1}", total_ocuppied_apartment_num, total_apartment_num);

            Logger.LogInfo(Logger.LOG_CAMPUS, "Dorms Apartment Capacity: {0} / {1}", total_ocuppied_apartment_num, total_apartment_num);

            var m_studentCapacityLabel = (UILabel)typeof(CampusWorldInfoPanel).GetField("m_studentCapacityLabel", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            if(m_studentCapacityLabel == null)
            {
                Logger.LogInfo(Logger.LOG_CAMPUS, "m_studentCapacityLabel is null");
                return;
            }

            Logger.LogInfo(Logger.LOG_CAMPUS, "old campus worker label: {0}", m_studentCapacityLabel);

            m_studentCapacityLabel.text = m_studentCapacityLabel.text + Environment.NewLine + dorms_capacity + Environment.NewLine;

            Logger.LogInfo(Logger.LOG_CAMPUS, "new campus worker label: {0}", m_studentCapacityLabel);

        }

    }
}
