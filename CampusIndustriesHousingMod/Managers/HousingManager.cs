using CampusIndustriesHousingMod.AI;
using ColossalFramework;
using System.Collections.Generic;

namespace CampusIndustriesHousingMod.Managers
{
    public static class HousingManager
    {
        internal static Dictionary<ushort, BuildingRecord> BuildingRecords;

        internal static List<PrefabRecord> PrefabRecords;

        public struct BuildingRecord
        {
            public string BuildingAI;
            public int NumOfApartments;
            public bool IsDefault;
            public bool IsPrefab;
            public bool IsGlobal;
            public bool IsLocked;
        }

        public struct PrefabRecord
        {
            public string InfoName;
            public string BuildingAI;
            public int NumOfApartments;
        }

        public static void Init()
        {
			BuildingRecords ??= [];
			PrefabRecords ??= [];
        }

        public static void Deinit()
        {
            BuildingRecords = [];
            PrefabRecords = [];
        }

        public static bool BuildingRecordExist(ushort buildingID)
        {
            if (BuildingRecords.ContainsKey(buildingID))
            {
                return true;
			}
            else
            {
                return false;
            }
        }

        public static BuildingRecord GetBuildingRecord(ushort buildingID)
        {
            if (BuildingRecords.TryGetValue(buildingID, out BuildingRecord buildingRecord))
            {
                return buildingRecord;
			}
            return default;
        }

        public static void SetBuildingRecord(ushort buildingID, BuildingRecord buildingRecord)
        {
            BuildingRecords[buildingID] = buildingRecord;
        }

        public static BuildingRecord CreateBuildingRecord(ushort buildingID)
        {
            Building building = Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID];
            PrefabAI buildingAI = building.Info.GetAI();
            string buildingAIstr = buildingAI.GetType().Name;

            BuildingRecord newBuildingRecord = new()
            {
                BuildingAI = buildingAIstr,
                IsDefault = true,
                IsPrefab = false,
                IsGlobal = false,
                IsLocked = true
            };

            if(buildingAI is BarracksAI barracksAI)
            {
                newBuildingRecord.NumOfApartments = DefaultValues(barracksAI.m_industryType);
            }
            else if (buildingAI is DormsAI dormsAI)
            {
                newBuildingRecord.NumOfApartments = DefaultValues(dormsAI.m_campusType);
            }

            BuildingRecords.Add(buildingID, newBuildingRecord);

            return newBuildingRecord;
        }

        public static void RemoveBuildingRecord(ushort buildingID)
        {
            // See if we've already got an entry for this building; if not, create one.
            if (BuildingRecords.TryGetValue(buildingID, out _))
            {
                // Init buildingRecord.
                BuildingRecords.Remove(buildingID);
            }
        }

        public static void ClearBuildingRecords()
        {
            BuildingRecords.Clear();
        }

        public static bool PrefabExist(string BuildingInfostr, string BuildingAIstr)
        {
            int index = PrefabRecords.FindIndex(item => item.InfoName == BuildingInfostr && item.BuildingAI == BuildingAIstr);
            return index != -1;
        }

        public static PrefabRecord GetPrefab(BuildingInfo buildingInfo)
        {
            string BuildingAIstr = buildingInfo.GetAI().GetType().Name;
            var index = PrefabRecords.FindIndex(item => item.InfoName == buildingInfo.name && item.BuildingAI == BuildingAIstr);
            if(index != -1)
            {
                return PrefabRecords[index];
            }
            return default;
        }

        public static void SetPrefab(PrefabRecord prefabRecord)
		{
            var index = PrefabRecords.FindIndex(item => item.InfoName == prefabRecord.InfoName && item.BuildingAI == prefabRecord.BuildingAI);
            if (index != -1) 
            {
                PrefabRecords[index] = prefabRecord;
            }
        }

        public static void CreatePrefab(PrefabRecord prefabRecord)
        {
            var index = PrefabRecords.FindIndex(item => item.InfoName == prefabRecord.InfoName && item.BuildingAI == prefabRecord.BuildingAI);
            if (index == -1)
            {
                PrefabRecords.Add(prefabRecord);
            }
        }

        public static void RemovePrefab(PrefabRecord prefabRecord)
        {
            var index = PrefabRecords.FindIndex(item => item.InfoName == prefabRecord.InfoName && item.BuildingAI == prefabRecord.BuildingAI);
            if(index != -1)
            {
                PrefabRecords.RemoveAt(index);
            }
        }

        public static void ClearPrefabRecords()
        {
            PrefabRecords.Clear();
        }

        public static int DefaultValues(DistrictPark.ParkType parkType)
        {
            if(parkType == DistrictPark.ParkType.Farming)
            {
                return 2;
            }
            else if(parkType == DistrictPark.ParkType.Forestry)
            {
                return 10;
            }
            else if(parkType == DistrictPark.ParkType.Oil)
            {
                return 50;
            }
            else if(parkType == DistrictPark.ParkType.Ore)
            {
                return 48;
            }
            else if (parkType == DistrictPark.ParkType.University)
            {
                return 60;
            }
            else if (parkType == DistrictPark.ParkType.LiberalArts)
            {
                return 60;
            }
            else if (parkType == DistrictPark.ParkType.TradeSchool)
            {
                return 60;
            }
            return 0;
        }

    }
}
