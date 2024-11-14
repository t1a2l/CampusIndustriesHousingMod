using System.Collections.Generic;
using ColossalFramework;
using CampusIndustriesHousingMod.Utils;
using CampusIndustriesHousingMod.AI;
using UnityEngine;
using System;

namespace CampusIndustriesHousingMod.Managers
{
    public static class UpdateBuildingSettings
    {
        public static void ChangeBuildingLockStatus(ushort buildingID, bool LockStatus)
        {
            var buildingRecord = HousingManager.GetBuildingRecord(buildingID);

            buildingRecord.IsLocked = LockStatus;

            HousingManager.SetBuildingRecord(buildingID, buildingRecord);
        }

        public static void SaveNewSettings(ushort buildingID, HousingManager.BuildingRecord record)
        {
            var buildingRecord = HousingManager.GetBuildingRecord(buildingID);

            buildingRecord.NumOfApartments = record.NumOfApartments;
            buildingRecord.IsDefault = false;
            buildingRecord.IsPrefab = false;
            buildingRecord.IsGlobal = false;

            HousingManager.SetBuildingRecord(buildingID, buildingRecord);
            UpdateBuildingCapacity(buildingID, buildingRecord.NumOfApartments, false);
        }

        public static void SetBuildingToPrefab(ushort buildingID, HousingManager.PrefabRecord recordPrefab)
        {
            var buildingRecord = HousingManager.GetBuildingRecord(buildingID);

            buildingRecord.NumOfApartments = recordPrefab.NumOfApartments;
            buildingRecord.IsDefault = false;
            buildingRecord.IsPrefab = true;
            buildingRecord.IsGlobal = false;

            HousingManager.SetBuildingRecord(buildingID, buildingRecord);
            UpdateBuildingCapacity(buildingID, buildingRecord.NumOfApartments, false);
        }

        public static void SetBuildingToGlobal(ushort buildingID, Housing buildingRecordGlobalConfig)
        {
            var buildingRecord = HousingManager.GetBuildingRecord(buildingID);

            buildingRecord.NumOfApartments = buildingRecordGlobalConfig.NumOfApartments;
            buildingRecord.IsDefault = false;
            buildingRecord.IsPrefab = false;
            buildingRecord.IsGlobal = true;

            HousingManager.SetBuildingRecord(buildingID, buildingRecord);
            UpdateBuildingCapacity(buildingID, buildingRecord.NumOfApartments, false);
        }

        public static void UpdateBuildingToDefaultSettings(ushort buildingID, HousingManager.BuildingRecord newDefaultRecord)
        {
            var buildingRecord = HousingManager.GetBuildingRecord(buildingID);

            buildingRecord.NumOfApartments = newDefaultRecord.NumOfApartments;
            buildingRecord.IsDefault = true;
            buildingRecord.IsPrefab = false;
            buildingRecord.IsGlobal = false;

            HousingManager.SetBuildingRecord(buildingID, buildingRecord);
            UpdateBuildingCapacity(buildingID, buildingRecord.NumOfApartments, false);
        }

        public static void CreatePrefabSettings(ushort buildingID, HousingManager.BuildingRecord newRecord)
        {
            var buildingInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info;
            string BuildingAIstr = buildingInfo.GetAI().GetType().Name;

            var buildingsIdsList = new List<ushort>();

            foreach (var item in HousingManager.BuildingRecords)
            {
                var Info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[item.Key].Info;
                if (Info.name == buildingInfo.name && Info.GetAI().GetType().Name == BuildingAIstr && !item.Value.IsLocked)
                {
                    buildingsIdsList.Add(item.Key);
                }
            }

            // set new prefab settings according to the building current settings
            var buildingRecordPrefab = new HousingManager.PrefabRecord
            {
                InfoName = buildingInfo.name,
                BuildingAI = BuildingAIstr,
                NumOfApartments = newRecord.NumOfApartments
            };

            foreach (ushort buildingId in buildingsIdsList)
            {
                var buildingRecord = HousingManager.GetBuildingRecord(buildingId);
                buildingRecord.NumOfApartments = buildingRecordPrefab.NumOfApartments;
                buildingRecord.IsDefault = false;
                buildingRecord.IsPrefab = true;
                buildingRecord.IsGlobal = false;
                HousingManager.SetBuildingRecord(buildingId, buildingRecord);
                UpdateBuildingCapacity(buildingID, buildingRecord.NumOfApartments, false);
            }

            if (HousingManager.PrefabExist(buildingInfo.name, buildingInfo.GetAI().GetType().Name))
            {
                // update the prefab
                var prefabRecord = HousingManager.GetPrefab(buildingInfo);

                prefabRecord.NumOfApartments = buildingRecordPrefab.NumOfApartments;

                HousingManager.SetPrefab(prefabRecord);
            }
            else
            {
                // create new prefab
                HousingManager.CreatePrefab(buildingRecordPrefab);
            }
        }

        public static void CreateGlobalSettings(ushort buildingID, HousingManager.BuildingRecord newRecord)
        {
            var buildingInfo = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID].Info;
            string BuildingAIstr = buildingInfo.GetAI().GetType().Name;

            var buildingsIdsList = new List<ushort>();

            foreach (var item in HousingManager.BuildingRecords)
            {
                var Info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[item.Key].Info;
                if (Info.name == buildingInfo.name && Info.GetAI().GetType().Name == BuildingAIstr && !item.Value.IsLocked)
                {
                    buildingsIdsList.Add(item.Key);
                }
            }

            // set new global settings according to the building current settings
            var buildingRecordGlobal = new Housing
            {
                Name = buildingInfo.name,
                BuildingAI = BuildingAIstr,
                NumOfApartments = newRecord.NumOfApartments
            };

            foreach (ushort buildingId in buildingsIdsList)
            {
                var buildingRecord = HousingManager.GetBuildingRecord(buildingId);
                buildingRecord.NumOfApartments = buildingRecordGlobal.NumOfApartments;
                buildingRecord.IsDefault = false;
                buildingRecord.IsPrefab = false;
                buildingRecord.IsGlobal = true;
                HousingManager.SetBuildingRecord(buildingId, buildingRecord);
                UpdateBuildingCapacity(buildingID, buildingRecord.NumOfApartments, false);
            }

            // try get global settings and update them or create new global settings for this building type
            // if not exist and apply the settings to all the individual buildings
            var globalRecord = HousingConfig.Config.GetGlobalSettings(buildingInfo);

            if (globalRecord != null)
            {
                globalRecord.NumOfApartments = buildingRecordGlobal.NumOfApartments;

                HousingConfig.Config.SetGlobalSettings(globalRecord);
            }
            else
            {
                HousingConfig.Config.CreateGlobalSettings(buildingRecordGlobal);
            }
        }

        public static void UpdateBuildingCapacity(ushort buildingID, int numOfApartments, bool is_new)
        {
            ref Building data = ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID];
            if (data.Info.GetAI() is BarracksAI barracksAI)
            {
                barracksAI.ValidateCapacity(buildingID, ref data, false);
                CreateOrEnsure(is_new, buildingID, ref data, barracksAI.GetModifiedCapacity(buildingID));
            }
            else if (data.Info.GetAI() is DormsAI dormsAI)
            {
                dormsAI.ValidateCapacity(buildingID, ref data, false);
                CreateOrEnsure(is_new, buildingID, ref data, dormsAI.GetModifiedCapacity(buildingID));
            }
        }

        private static void CreateOrEnsure(bool is_new, ushort buildingID, ref Building data, int numOfApartments)
        {
            if (is_new)
            {
                Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, numOfApartments);
            }
            else
            {
                EnsureCitizenUnits(buildingID, ref data, numOfApartments);
            }
        }

        private static void EnsureCitizenUnits(ushort buildingID, ref Building data, int homeCount = 0)
        {
            bool old_building = false;
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
                if ((flags & CitizenUnit.Flags.Work) != 0 || (flags & CitizenUnit.Flags.Student) != 0)
                {
                    old_building = true;
                    break;
                }
                num = num2;
                num2 = instance.m_units.m_buffer[num2].m_nextUnit;
                if (++num3 > 524288)
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            if (old_building) 
            {
                Singleton<CitizenManager>.instance.ReleaseUnits(data.m_citizenUnits);
                instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, homeCount);
                return;
            }
            homeCount = Mathf.Max(0, homeCount);
            if (homeCount == 0)
            {
                return;
            }
            if (instance.CreateUnits(out uint firstUnit, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, homeCount))
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