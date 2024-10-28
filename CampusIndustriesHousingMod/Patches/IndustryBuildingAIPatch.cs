using CampusIndustriesHousingMod.AI;
using ColossalFramework;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CampusIndustriesHousingMod.Patches
{
	[HarmonyPatch(typeof(IndustryBuildingAI))]
	public static class IndustryBuildingAIPatch
	{
		private delegate void PlayerBuildingAICreateBuildingDelegate(PlayerBuildingAI __instance, ushort buildingID, ref Building data);
        private static PlayerBuildingAICreateBuildingDelegate BaseCreateBuilding = AccessTools.MethodDelegate<PlayerBuildingAICreateBuildingDelegate>(typeof(PlayerBuildingAI).GetMethod("CreateBuilding", BindingFlags.Instance | BindingFlags.Public), null, false);

		private delegate void PlayerBuildingAIBuildingLoadedDelegate(PlayerBuildingAI __instance, ushort buildingID, ref Building data, uint version);
        private static PlayerBuildingAIBuildingLoadedDelegate BaseBuildingLoaded = AccessTools.MethodDelegate<PlayerBuildingAIBuildingLoadedDelegate>(typeof(PlayerBuildingAI).GetMethod("BuildingLoaded", BindingFlags.Instance | BindingFlags.Public), null, false);

		private delegate void PlayerBuildingAIEndRelocatingDelegate(PlayerBuildingAI __instance, ushort buildingID, ref Building data);
        private static PlayerBuildingAIEndRelocatingDelegate BaseEndRelocating = AccessTools.MethodDelegate<PlayerBuildingAIEndRelocatingDelegate>(typeof(PlayerBuildingAI).GetMethod("EndRelocating", BindingFlags.Instance | BindingFlags.Public), null, false);

        [HarmonyPatch(typeof(IndustryBuildingAI), "CreateBuilding")]
        [HarmonyPrefix]
        public static bool CreateBuilding(IndustryBuildingAI __instance, ushort buildingID, ref Building data, ref Dictionary<uint, FastList<IndustryBuildingAI>> ___m_searchTable, ref Dictionary<uint, int> ___m_lastTableIndex)
        {
            if(data.Info.GetAI() is BarracksAI)
            {
                var SearchKey = (uint)typeof(IndustryBuildingAI).GetProperty("SearchKey", AccessTools.all).GetValue(__instance, null);
                if (__instance.m_info.m_placementStyle == ItemClass.Placement.Manual && ___m_searchTable.TryGetValue(SearchKey, out var value) && value != null && value.m_size >= 2)
                {
                    if (___m_lastTableIndex[SearchKey] < 0)
                    {
                        ___m_lastTableIndex[SearchKey] = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)value.m_size);
                    }
                    IndustryBuildingAI industryBuildingAI = value.m_buffer[___m_lastTableIndex[SearchKey]];
                    if (industryBuildingAI != null && industryBuildingAI.m_info.m_placementStyle == ItemClass.Placement.Procedural)
                    {
                        data.Info = industryBuildingAI.m_info;
                        industryBuildingAI.CreateBuilding(buildingID, ref data);
                        return false;
                    }
                }
                BaseCreateBuilding(__instance, buildingID, ref data);
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte b = instance.GetPark(data.m_position);
                if (b != 0)
                {
                    if (!instance.m_parks.m_buffer[b].IsIndustry)
                    {
                        b = 0;
                    }
                    else if (__instance.m_industryType == DistrictPark.ParkType.Industry || __instance.m_industryType != instance.m_parks.m_buffer[b].m_parkType)
                    {
                        b = 0;
                    }
                }
                instance.AddParkBuilding(b, __instance.m_info, __instance.m_industryType);
                instance.m_industryAreaCreated.Disable();
                if (b == 0 && __instance.m_industryType != DistrictPark.ParkType.Industry && Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info.m_class.m_service != ItemClass.Service.Fishing)
                {
                    data.m_problems = Notification.AddProblems(data.m_problems, Notification.Problem1.NotInIndustryArea);
                    GuideController properties = Singleton<GuideManager>.instance.m_properties;
                    if (properties is not null)
                    {
                        Singleton<BuildingManager>.instance.m_industryBuildingOutsideIndustryArea.Activate(properties.m_industryBuildingOutsideIndustryArea, buildingID);
                    }
                }
                return false;
            }
            return true;
        }

		[HarmonyPatch(typeof(IndustryBuildingAI), "BuildingLoaded")]
        [HarmonyPrefix]
		public static bool BuildingLoaded(IndustryBuildingAI __instance, ushort buildingID, ref Building data, uint version)
		{
            if (data.Info.GetAI() is BarracksAI)
            {
                BaseBuildingLoaded(__instance, buildingID, ref data, version);
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte b = instance.GetPark(data.m_position);
                if (b != 0)
                {
                    if (!instance.m_parks.m_buffer[b].IsIndustry)
                    {
                        b = 0;
                    }
                    else if (__instance.m_industryType == DistrictPark.ParkType.Industry || __instance.m_industryType != instance.m_parks.m_buffer[b].m_parkType)
                    {
                        b = 0;
                    }
                }
                instance.AddParkBuilding(b, __instance.m_info, __instance.m_industryType);
                if (__instance.m_industryType != DistrictPark.ParkType.Industry)
                {
                    if (b == 0 && Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info.m_class.m_service != ItemClass.Service.Fishing)
                    {
                        data.m_problems = Notification.AddProblems(data.m_problems, Notification.Problem1.NotInIndustryArea);
                    }
                    else
                    {
                        data.m_problems = Notification.RemoveProblems(data.m_problems, Notification.Problem1.NotInIndustryArea);
                    }
                }
                return false;
            }
            return true;
        }

		[HarmonyPatch(typeof(IndustryBuildingAI), "EndRelocating")]
        [HarmonyPrefix]
		public static bool EndRelocating(IndustryBuildingAI __instance, ushort buildingID, ref Building data)
		{
            if (data.Info.GetAI() is BarracksAI)
            {
                BaseEndRelocating(__instance, buildingID, ref data);
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte b = instance.GetPark(data.m_position);
                if (b != 0)
                {
                    if (!instance.m_parks.m_buffer[b].IsIndustry)
                    {
                        b = 0;
                    }
                    else if (__instance.m_industryType == DistrictPark.ParkType.Industry || __instance.m_industryType != instance.m_parks.m_buffer[b].m_parkType)
                    {
                        b = 0;
                    }
                }
                instance.AddParkBuilding(b, __instance.m_info, __instance.m_industryType);
                if (__instance.m_industryType != DistrictPark.ParkType.Industry)
                {
                    if (b == 0)
                    {
                        AddAreaNotification(buildingID, ref data);
                    }
                    else
                    {
                        RemoveAreaNotification(buildingID, ref data);
                    }
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(IndustryBuildingAI), "HandleCrime")]
        [HarmonyPrefix]
        public static bool HandleCrime(IndustryBuildingAI __instance, ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            DistrictManager instance = Singleton<DistrictManager>.instance;
            BuildingManager instance2 = Singleton<BuildingManager>.instance;
            byte b = instance.GetPark(data.m_position);
            if (b != 0)
            {
                if (!instance.m_parks.m_buffer[b].IsIndustry)
                {
                    b = 0;
                }
                else if (__instance.m_industryType == DistrictPark.ParkType.Industry || __instance.m_industryType != instance.m_parks.m_buffer[b].m_parkType)
                {
                    b = 0;
                }
            }
            ushort num = 0;
            if (b != 0)
            {
                num = instance.m_parks.m_buffer[b].m_randomGate;
                if (num == 0)
                {
                    num = instance.m_parks.m_buffer[b].m_mainGate;
                }
            }
            if (num == 0 || FindRoadAccess(buildingID, ref data, data.CalculateSidewalkPosition(), out var _))
            {
                BaseHandleCrime(__instance, buildingID, ref data, crimeAccumulation, citizenCount);
                return false;
            }
            bool flag = (instance2.m_buildings.m_buffer[num].m_flags & Building.Flags.Active) == 0;
            bool flag2 = (instance2.m_buildings.m_buffer[num].m_flags & Building.Flags.RateReduced) != 0;
            if (crimeAccumulation != 0)
            {
                if (Singleton<SimulationManager>.instance.m_isNightTime)
                {
                    crimeAccumulation = crimeAccumulation * 5 >> 2;
                }
                if (data.m_eventIndex != 0)
                {
                    EventManager instance3 = Singleton<EventManager>.instance;
                    EventInfo info = instance3.m_events.m_buffer[data.m_eventIndex].Info;
                    crimeAccumulation = info.m_eventAI.GetCrimeAccumulation(data.m_eventIndex, ref instance3.m_events.m_buffer[data.m_eventIndex], crimeAccumulation);
                }
                crimeAccumulation = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)crimeAccumulation);
                crimeAccumulation = UniqueFacultyAI.DecreaseByBonus(UniqueFacultyAI.FacultyBonus.Law, crimeAccumulation);
                if (!Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.PoliceDepartment))
                {
                    crimeAccumulation = 0;
                }
            }
            data.m_crimeBuffer = (ushort)Mathf.Min(citizenCount * 100, data.m_crimeBuffer + crimeAccumulation);
            ushort num2 = (ushort)Mathf.Min(data.m_crimeBuffer, 65535 - instance2.m_buildings.m_buffer[num].m_crimeBuffer);
            if (flag)
            {
                num2 = 0;
            }
            else if (flag2)
            {
                num2 = (ushort)Mathf.Min(num2, crimeAccumulation >> 1);
            }
            instance2.m_buildings.m_buffer[num].m_crimeBuffer += num2;
            data.m_crimeBuffer -= num2;
            Notification.ProblemStruct problemStruct = Notification.RemoveProblems(data.m_problems, Notification.Problem1.Crime);
            if (data.m_crimeBuffer > citizenCount * 90)
            {
                problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Crime | Notification.Problem1.MajorProblem);
            }
            else if (data.m_crimeBuffer > citizenCount * 60)
            {
                problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Crime);
            }
            data.m_problems = problemStruct;
			return false;
        }

        private static void AddAreaNotification(ushort buildingID, ref Building data)
		{
			Notification.ProblemStruct problems = data.m_problems;
			Notification.ProblemStruct problemStruct = Notification.AddProblems(data.m_problems, Notification.Problem1.NotInIndustryArea);
			if (problems != problemStruct)
			{
				data.m_problems = problemStruct;
				Singleton<BuildingManager>.instance.UpdateNotifications(buildingID, problems, problemStruct);
			}
		}

		private static void RemoveAreaNotification(ushort buildingID, ref Building data)
		{
			Notification.ProblemStruct problems = data.m_problems;
			Notification.ProblemStruct problemStruct = Notification.RemoveProblems(data.m_problems, Notification.Problem1.NotInIndustryArea);
			if (problems != problemStruct)
			{
				data.m_problems = problemStruct;
				Singleton<BuildingManager>.instance.UpdateNotifications(buildingID, problems, problemStruct);
			}
		}

        private static bool FindRoadAccess(ushort buildingID, ref Building data, Vector3 position, out ushort segmentID, bool mostCloser = false, bool untouchable = true)
        {
            Bounds bounds = new(position, new Vector3(40f, 40f, 40f));
            int num = Mathf.Max((int)((bounds.min.x - 64f) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((bounds.min.z - 64f) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((bounds.max.x + 64f) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((bounds.max.z + 64f) / 64f + 135f), 269);
            segmentID = 0;
            float num5 = float.MaxValue;
            NetManager instance = Singleton<NetManager>.instance;
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    int num6 = 0;
                    for (ushort num7 = instance.m_segmentGrid[i * 270 + j]; num7 != 0; num7 = instance.m_segments.m_buffer[num7].m_nextGridSegment)
                    {
                        if (num6++ >= 36864)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                        NetInfo info = instance.m_segments.m_buffer[num7].Info;
                        if (info.m_class.m_service == ItemClass.Service.Road && !info.m_netAI.IsUnderground() && !info.m_netAI.IsOverground() && info.m_netAI is RoadBaseAI && (untouchable || (instance.m_segments.m_buffer[num7].m_flags & NetSegment.Flags.Untouchable) == 0) && info.m_hasPedestrianLanes && (info.m_hasForwardVehicleLanes || info.m_hasBackwardVehicleLanes))
                        {
                            ushort startNode = instance.m_segments.m_buffer[num7].m_startNode;
                            ushort endNode = instance.m_segments.m_buffer[num7].m_endNode;
                            Vector3 position2 = instance.m_nodes.m_buffer[startNode].m_position;
                            Vector3 position3 = instance.m_nodes.m_buffer[endNode].m_position;
                            float num8 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position2.x, bounds.min.z - 64f - position2.z), Mathf.Max(position2.x - bounds.max.x - 64f, position2.z - bounds.max.z - 64f));
                            float num9 = Mathf.Max(Mathf.Max(bounds.min.x - 64f - position3.x, bounds.min.z - 64f - position3.z), Mathf.Max(position3.x - bounds.max.x - 64f, position3.z - bounds.max.z - 64f));
                            if ((!(num8 >= 0f) || !(num9 >= 0f)) && instance.m_segments.m_buffer[num7].m_bounds.Intersects(bounds) && instance.m_segments.m_buffer[num7].GetClosestLanePosition(position, NetInfo.LaneType.Vehicle | NetInfo.LaneType.TransportVehicle, VehicleInfo.VehicleType.Car, VehicleInfo.VehicleCategory.RoadTransport, VehicleInfo.VehicleType.None, requireConnect: false, out var positionA, out var _, out var _, out var _, out var _, out var _))
                            {
                                float num10 = Vector3.SqrMagnitude(position - positionA);
                                if (!(num10 >= 400f) && !(num10 >= num5))
                                {
                                    segmentID = num7;
                                    if (!mostCloser)
                                    {
                                        return true;
                                    }
                                    num5 = num10;
                                }
                            }
                        }
                    }
                }
            }
            if (segmentID == 0)
            {
                data.m_flags |= Building.Flags.RoadAccessFailed;
                return false;
            }
            return true;
        }

        private static void BaseHandleCrime(CommonBuildingAI __instance, ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount)
        {
            if (crimeAccumulation != 0)
            {
                byte park = Singleton<DistrictManager>.instance.GetPark(data.m_position);
                if (park != 0 && (Singleton<DistrictManager>.instance.m_parks.m_buffer[park].m_parkPolicies & DistrictPolicies.Park.SugarBan) != 0)
                {
                    crimeAccumulation = (int)((float)crimeAccumulation * 1.2f);
                }
                if (Singleton<SimulationManager>.instance.m_isNightTime)
                {
                    crimeAccumulation = crimeAccumulation * 5 >> 2;
                }
                if (data.m_eventIndex != 0)
                {
                    EventManager instance = Singleton<EventManager>.instance;
                    EventInfo info = instance.m_events.m_buffer[data.m_eventIndex].Info;
                    crimeAccumulation = info.m_eventAI.GetCrimeAccumulation(data.m_eventIndex, ref instance.m_events.m_buffer[data.m_eventIndex], crimeAccumulation);
                }
                crimeAccumulation = Singleton<SimulationManager>.instance.m_randomizer.Int32((uint)crimeAccumulation);
                crimeAccumulation = UniqueFacultyAI.DecreaseByBonus(UniqueFacultyAI.FacultyBonus.Law, crimeAccumulation);
                if (!Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.PoliceDepartment))
                {
                    crimeAccumulation = 0;
                }
            }
            data.m_crimeBuffer = (ushort)Mathf.Min(citizenCount * 100, data.m_crimeBuffer + crimeAccumulation);
            int crimeBuffer = data.m_crimeBuffer;
            if (citizenCount != 0 && crimeBuffer > citizenCount * 25 && Singleton<SimulationManager>.instance.m_randomizer.Int32(5u) == 0)
            {
                int count = 0;
                int cargo = 0;
                int capacity = 0;
                int outside = 0;
                __instance.CalculateGuestVehicles(buildingID, ref data, TransferManager.TransferReason.Crime, ref count, ref cargo, ref capacity, ref outside);
                if (count == 0)
                {
                    TransferManager.TransferOffer offer = default;
                    offer.Priority = crimeBuffer / Mathf.Max(1, citizenCount * 10);
                    offer.Building = buildingID;
                    offer.Position = data.m_position;
                    offer.Amount = 1;
                    Singleton<TransferManager>.instance.AddOutgoingOffer(TransferManager.TransferReason.Crime, offer);
                }
            }
            SetCrimeNotification(ref data, citizenCount);
        }

        private static void SetCrimeNotification(ref Building data, int citizenCount)
        {
            Notification.ProblemStruct problemStruct = Notification.RemoveProblems(data.m_problems, Notification.Problem1.Crime);
            if (data.m_crimeBuffer > citizenCount * 90)
            {
                problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Crime | Notification.Problem1.MajorProblem);
            }
            else if (data.m_crimeBuffer > citizenCount * 60)
            {
                problemStruct = Notification.AddProblems(problemStruct, Notification.Problem1.Crime);
            }
            data.m_problems = problemStruct;
        }
    }
}
