﻿using System;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace CampusIndustriesHousingMod.Managers
{
    public class MoveInProbabilityHelper
    {
        private static readonly float BASE_CHANCE_VALUE = 0f;
        private static readonly float DISTANCE_MAX_CHANCE_VALUE = 100f;
        private static readonly float QUALITY_MAX_CHANCE_VALUE = 200f;
        private static readonly float MAX_CHANCE_VALUE = DISTANCE_MAX_CHANCE_VALUE + QUALITY_MAX_CHANCE_VALUE;
        private static readonly float NO_CHANCE = -(MAX_CHANCE_VALUE * 10);

        public static bool CheckIfShouldMoveIn(uint[] family, ref Building buildingData, ref Randomizer randomizer, string type)
        {
            float chanceValue = BASE_CHANCE_VALUE;
            Logger.LogInfo(Logger.LOG_CHANCES, "MoveInProbabilityHelper.CheckIfShouldMoveIn -- Base Chance: {0}", chanceValue);
            Logger.LogInfo(Logger.LOG_CHANCES, "---------------------------------");

            // Distance
            chanceValue += GetDistanceChanceValue(family, ref buildingData, type);

            // Wealth
            chanceValue += GetWealthChanceValue(family, type);

            // Check for no chance
            if (chanceValue <= 0)
            {
                Logger.LogInfo(Logger.LOG_CHANCES, "MoveInProbabilityHelper.CheckIfShouldMoveIn -- No Chance: {0}", chanceValue);
                return false;
            }

            // Check against random value
            uint maxChance = (uint)MAX_CHANCE_VALUE;
            int randomValue = randomizer.Int32(maxChance);
            Logger.LogInfo(Logger.LOG_CHANCES, "MoveInProbabilityHelper.CheckIfShouldMoveIn -- Total Chance Value: {0} -- Random Number: {1} -- result: {2}", chanceValue, randomValue, randomValue <= chanceValue);
            return randomValue <= chanceValue;
        }

        private static float GetDistanceChanceValue(uint[] family, ref Building buildingData, string type)
        {
            WorkerManager workerManager = WorkerManager.GetInstance();
            StudentManager studentManager = StudentManager.GetInstance();

            // Get the home for the family
            ushort homeBuildingId = GetHomeBuildingIdForFamily(family);

            if (homeBuildingId == 0)
            {
                // homeBuilding should never be 0, but if it is return NO_CHANCE to prevent this family from being chosen 
                Logger.LogError(Logger.LOG_CHANCES, "MoveInProbabilityHelper.GetDistanceChanceValue -- Home Building was 0 when it shouldn't have been");
                return NO_CHANCE;
            }

            ushort workBuildingId = 0;
            if (type == "worker")
            {
                foreach (uint familyMember in family)
                {
                    if (workerManager.IsIndustryAreaWorker(familyMember))
                    {
                        workBuildingId = GetWorkBuildingId(familyMember);
                        break;
                    }
                }
            }
            else if (type == "student")
            {
                foreach (uint familyMember in family)
                {
                    if (studentManager.IsCampusAreaStudent(familyMember))
                    {
                        workBuildingId = GetWorkBuildingId(familyMember);
                        break;
                    }
                }
            }


            if (workBuildingId == 0)
            {
                // workBuildingId should never be 0, but if it is return NO_CHANCE to prevent this family from being chosen 
                Logger.LogError(Logger.LOG_CHANCES, "MoveInProbabilityHelper.GetDistanceChanceValue -- Work Building was 0 when it shouldn't have been");
                return NO_CHANCE;
            }

            Building homeBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[homeBuildingId];
            Building workBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[workBuildingId];

            // Get the distance between current home and work and the new home and work and check which is closer
            float distance_from_current_home_to_work = Vector3.Distance(homeBuilding.m_position, workBuilding.m_position);
            float distance_from_new_home_to_Work = Vector3.Distance(buildingData.m_position, workBuilding.m_position);

            float distanceChanceValue;
            // Calulate the chance modifier based on distances
            if (distance_from_current_home_to_work <= distance_from_new_home_to_Work || distance_from_current_home_to_work <= 500f)
            {
                distanceChanceValue = DISTANCE_MAX_CHANCE_VALUE * 0.25f;
            }
            else
            {
                distanceChanceValue = DISTANCE_MAX_CHANCE_VALUE * 1f;
            }
            Logger.LogInfo(Logger.LOG_CHANCES, "MoveInProbabilityHelper.GetDistanceChanceValue -- Distance Chance Value: {0} -- Distance From Current Home: {1}, Distance From New Home: {2}", distanceChanceValue, distance_from_current_home_to_work, distance_from_new_home_to_Work);

            return distanceChanceValue;
        }

        private static ushort GetHomeBuildingIdForFamily(uint[] family)
        {
            foreach (uint familyMember in family)
            {
                if (familyMember != 0)
                {
                    return Singleton<CitizenManager>.instance.m_citizens.m_buffer[familyMember].m_homeBuilding;
                }
            }

            return 0;
        }

        private static ushort GetWorkBuildingId(uint familyMember)
        {
            return Singleton<CitizenManager>.instance.m_citizens.m_buffer[familyMember].m_workBuilding;
        }

        private static float GetWealthChanceValue(uint[] family, string type)
        {
            Citizen.Wealth wealth = GetFamilyWealth(family);
            float chance = NO_CHANCE;
            if (type == "worker")
            {
                switch (wealth)
                {
                    case Citizen.Wealth.High:
                        chance = QUALITY_MAX_CHANCE_VALUE * 0.25f; // low chance
                        break;
                    case Citizen.Wealth.Medium:
                        chance = QUALITY_MAX_CHANCE_VALUE * 1f; // normal chance 
                        break;
                    case Citizen.Wealth.Low:
                        chance = QUALITY_MAX_CHANCE_VALUE * 2f; // high chance
                        break;
                }
            }
            if (type == "student")
            {
                switch (wealth)
                {
                    case Citizen.Wealth.High:
                        chance = QUALITY_MAX_CHANCE_VALUE * 0.25f; // low chance
                        break;
                    case Citizen.Wealth.Medium:
                        chance = QUALITY_MAX_CHANCE_VALUE * 1f; // normal chance 
                        break;
                    case Citizen.Wealth.Low:
                        chance = QUALITY_MAX_CHANCE_VALUE * 1f; // normal chance 
                        break;
                }
            }

            Logger.LogInfo(Logger.LOG_CHANCES, "MoveInProbabilityHelper.GetQualityLevelChanceValue -- Wealth Chance Value: {0} -- Family Wealth: {1} -- type: {2}", chance, wealth, type);
            return chance;
        }

        private static Citizen.Wealth GetFamilyWealth(uint[] family)
        {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;

            // Get the average wealth of all young adult, adults and seniors in the house
            int total = 0;
            int numCounted = 0;
            foreach (uint familyMember in family)
            {
                if (familyMember != 0)
                {
                    if (citizenManager.m_citizens.m_buffer[familyMember].Age > Citizen.AGE_LIMIT_YOUNG)
                    {
                        total += (int)citizenManager.m_citizens.m_buffer[familyMember].WealthLevel;
                        numCounted++;
                    }
                }
            }

            // Should never happen but prevent possible division by 0
            if (numCounted == 0)
            {
                return Citizen.Wealth.Low;
            }

            int wealthValue = Convert.ToInt32(Math.Round(total / (double)numCounted, MidpointRounding.AwayFromZero));
            return (Citizen.Wealth)wealthValue;
        }

    }
}