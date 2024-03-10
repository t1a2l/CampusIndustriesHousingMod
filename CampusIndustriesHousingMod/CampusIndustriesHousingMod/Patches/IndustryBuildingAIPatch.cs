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

        private delegate void CommonBuildingAIHandleCrimeDelegate(CommonBuildingAI __instance, ushort buildingID, ref Building data, int crimeAccumulation, int citizenCount);
        private static CommonBuildingAIHandleCrimeDelegate BaseHandleCrime = AccessTools.MethodDelegate<CommonBuildingAIHandleCrimeDelegate>(typeof(CommonBuildingAI).GetMethod("HandleCrime", BindingFlags.Instance | BindingFlags.NonPublic), null, true);

        private delegate bool PlayerBuildingAIFindRoadAccessDelegate(PlayerBuildingAI __instance, ushort buildingID, ref Building data, Vector3 position, out ushort segmentID, bool mostCloser = false, bool untouchable = true);
        private static PlayerBuildingAIFindRoadAccessDelegate BaseFindRoadAccesse = AccessTools.MethodDelegate<PlayerBuildingAIFindRoadAccessDelegate>(typeof(PlayerBuildingAI).GetMethod("FindRoadAccess", BindingFlags.Instance | BindingFlags.Public), null, true);


        [HarmonyPatch(typeof(IndustryBuildingAI), "CreateBuilding")]
        [HarmonyPrefix]
        public static bool CreateBuilding(IndustryBuildingAI __instance, ushort buildingID, ref Building data, ref Dictionary<uint, FastList<IndustryBuildingAI>> ___m_searchTable, ref Dictionary<uint, int> ___m_lastTableIndex)
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
			if(data.Info.GetAI() is not BarracksAI)
			{
				int workCount = __instance.m_workPlaceCount0 + __instance.m_workPlaceCount1 + __instance.m_workPlaceCount2 + __instance.m_workPlaceCount3;
				Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, 0, workCount);
			}
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
				if ((object)properties != null)
				{
					Singleton<BuildingManager>.instance.m_industryBuildingOutsideIndustryArea.Activate(properties.m_industryBuildingOutsideIndustryArea, buildingID);
				}
			}
            return false;
        }

		[HarmonyPatch(typeof(IndustryBuildingAI), "BuildingLoaded")]
        [HarmonyPrefix]
		public static bool BuildingLoaded(IndustryBuildingAI __instance, ushort buildingID, ref Building data, uint version)
		{
			BaseBuildingLoaded(__instance, buildingID, ref data, version);
			if(data.Info.GetAI() is not BarracksAI)
			{
				int workCount = __instance.m_workPlaceCount0 + __instance.m_workPlaceCount1 + __instance.m_workPlaceCount2 + __instance.m_workPlaceCount3;
				EnsureCitizenUnits(buildingID, ref data, 0, workCount);
			}
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

		[HarmonyPatch(typeof(IndustryBuildingAI), "EndRelocating")]
        [HarmonyPrefix]
		public static bool EndRelocating(IndustryBuildingAI __instance, ushort buildingID, ref Building data)
		{
			BaseEndRelocating(__instance, buildingID, ref data);
			if(data.Info.GetAI() is not BarracksAI)
			{
				int workCount = __instance.m_workPlaceCount0 + __instance.m_workPlaceCount1 + __instance.m_workPlaceCount2 + __instance.m_workPlaceCount3;
				EnsureCitizenUnits(buildingID, ref data, 0, workCount);
			}
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
            if (num == 0 || BaseFindRoadAccesse(__instance, buildingID, ref data, data.CalculateSidewalkPosition(), out var _))
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

		private static void EnsureCitizenUnits(ushort buildingID, ref Building data, int homeCount = 0, int workCount = 0, int visitCount = 0, int studentCount = 0, int hotelCount = 0)
		{
			if ((data.m_flags & (Building.Flags.Abandoned | Building.Flags.Collapsed)) != 0)
			{
				return;
			}
			Citizen.Wealth wealthLevel = Citizen.GetWealthLevel((ItemClass.Level)data.m_level);
			CitizenManager instance = Singleton<CitizenManager>.instance;
			uint num = 0u;
			uint num2 = data.m_citizenUnits;
			int num3 = 0;
			while (num2 != 0)
			{
				CitizenUnit.Flags flags = instance.m_units.m_buffer[num2].m_flags;
				if ((flags & CitizenUnit.Flags.Home) != 0)
				{
					instance.m_units.m_buffer[num2].SetWealthLevel(wealthLevel);
					homeCount--;
				}
				if ((flags & CitizenUnit.Flags.Work) != 0)
				{
					workCount -= 5;
				}
				if ((flags & CitizenUnit.Flags.Visit) != 0)
				{
					visitCount -= 5;
				}
				if ((flags & CitizenUnit.Flags.Student) != 0)
				{
					studentCount -= 5;
				}
				num = num2;
				num2 = instance.m_units.m_buffer[num2].m_nextUnit;
				if (++num3 > 524288)
				{
					CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
					break;
				}
			}
			homeCount = Mathf.Max(0, homeCount);
			workCount = Mathf.Max(0, workCount);
			visitCount = Mathf.Max(0, visitCount);
			studentCount = Mathf.Max(0, studentCount);
			hotelCount = Mathf.Max(0, hotelCount);
			if (homeCount == 0 && workCount == 0 && visitCount == 0 && studentCount == 0 && hotelCount == 0)
			{
				return;
			}
			if (instance.CreateUnits(out uint firstUnit, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, homeCount, workCount, visitCount, 0, studentCount, hotelCount))
			{
				if (num != 0)
				{
					instance.m_units.m_buffer[num].m_nextUnit = firstUnit;
				}
				else
				{
					data.m_citizenUnits = firstUnit;
				}
			}
		}
	}
}
