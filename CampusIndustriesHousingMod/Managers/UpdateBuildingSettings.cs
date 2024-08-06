using System.Collections.Generic;
using ColossalFramework;
using CampusIndustriesHousingMod.Utils;

namespace CampusIndustriesHousingMod.Managers
{
    internal static class UpdateBuildingSettings
    {
        internal static void ChangeBuildingLockStatus(ushort buildingID, bool LockStatus)
        {
            var buildingRecord = HousingManager.GetBuildingRecord(buildingID);

            buildingRecord.IsLocked = LockStatus;

            HousingManager.SetBuildingRecord(buildingID, buildingRecord);
        }

        internal static void SaveNewSettings(ushort buildingID, HousingManager.BuildingRecord record)
        {
            var buildingRecord = HousingManager.GetBuildingRecord(buildingID);

            buildingRecord.NumOfApartments = record.NumOfApartments;
            buildingRecord.IsDefault = false;
            buildingRecord.IsPrefab = false;
            buildingRecord.IsGlobal = false;

            HousingManager.SetBuildingRecord(buildingID, buildingRecord);
        }

        internal static void SetBuildingToPrefab(ushort buildingID, HousingManager.PrefabRecord recordPrefab)
        {
            var buildingRecord = HousingManager.GetBuildingRecord(buildingID);

            buildingRecord.NumOfApartments = recordPrefab.NumOfApartments;
            buildingRecord.IsDefault = false;
            buildingRecord.IsPrefab = true;
            buildingRecord.IsGlobal = false;

            HousingManager.SetBuildingRecord(buildingID, buildingRecord);
        }

        internal static void SetBuildingToGlobal(ushort buildingID, Housing buildingRecordGlobalConfig)
        {
            var buildingRecord = HousingManager.GetBuildingRecord(buildingID);

            buildingRecord.NumOfApartments = buildingRecordGlobalConfig.NumOfApartments;
            buildingRecord.IsDefault = false;
            buildingRecord.IsPrefab = false;
            buildingRecord.IsGlobal = true;

            HousingManager.SetBuildingRecord(buildingID, buildingRecord);
        }

        internal static void UpdateBuildingToDefaultSettings(ushort buildingID, HousingManager.BuildingRecord newDefaultRecord)
        {
            var buildingRecord = HousingManager.GetBuildingRecord(buildingID);

            buildingRecord.NumOfApartments = newDefaultRecord.NumOfApartments;
            buildingRecord.IsDefault = true;
            buildingRecord.IsPrefab = false;
            buildingRecord.IsGlobal = false;

            HousingManager.SetBuildingRecord(buildingID, buildingRecord);
        }

        internal static void CreatePrefabSettings(ushort buildingID, HousingManager.BuildingRecord newRecord)
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
            }

            if (HousingManager.PrefabExist(buildingInfo))
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

        internal static void CreateGlobalSettings(ushort buildingID, HousingManager.BuildingRecord newRecord)
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

    }
}