﻿using CampusIndustriesHousingMod.AI;
using System.Collections.Generic;

namespace CampusIndustriesHousingMod.Managers
{
    public static class HousingManager
    {
        internal static Dictionary<uint, BuildingRecord> BuildingRecords;

        internal static List<PrefabRecord> PrefabRecords;

        public struct BuildingRecord
        {
            public string BuildingAI;

            public int NumOfApartments;

            public bool DefaultValues;
        }

        public struct PrefabRecord
        {
            public string Name;

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
            if (BuildingRecords.TryGetValue(buildingID, out BuildingRecord buildingRecord))
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
            else
            {
                BuildingRecord newBuildingRecord = new();
                BuildingRecords.Add(buildingID, newBuildingRecord);
                return newBuildingRecord;
            }
        }

        public static void SetBuildingRecord(ushort buildingID, BuildingRecord buildingRecord)
        {
            BuildingRecords[buildingID] = buildingRecord;
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

        public static PrefabRecord GetPrefab(string name, string buildingAI)
        {
            var index = PrefabRecords.FindIndex(item => item.Name == name && item.BuildingAI == buildingAI);
            if(index != -1)
            {
                return PrefabRecords[index];
            }
            else
            {
				PrefabRecord newPrefabRecord = new()
				{
					Name = name,
                    BuildingAI = buildingAI
				};
				PrefabRecords.Add(newPrefabRecord);
                return newPrefabRecord;
            }
        }

        public static void SetPrefab(PrefabRecord prefabRecord)
		{
            var index = PrefabRecords.FindIndex(item => item.Name == prefabRecord.Name && item.BuildingAI == prefabRecord.BuildingAI);
            PrefabRecords[index] = prefabRecord;
        }

        public static void RemovePrefab(string name, string buildingAI)
        {
            var index = PrefabRecords.FindIndex(item => item.Name == name && item.BuildingAI == buildingAI);
            if(index != -1)
            {
                PrefabRecords.RemoveAt(index);
            }
        }

        public static void ClearPrefabRecords()
        {
            PrefabRecords.Clear();
        }

        public static BarracksAI DefaultBarracksValues(BarracksAI barracks)
        {
            if(barracks.m_industryType == DistrictPark.ParkType.Farming)
            {
                barracks.numApartments = 2;
            }
            else if(barracks.m_industryType == DistrictPark.ParkType.Forestry)
            {
                barracks.numApartments = 10;
            }
            else if(barracks.m_industryType == DistrictPark.ParkType.Oil)
            {
                barracks.numApartments = 50;
            }
            else if(barracks.m_industryType == DistrictPark.ParkType.Ore)
            {
                barracks.numApartments = 48;
            }

            return barracks;
        }

        public static DormsAI DefaultDormsValues(DormsAI dorms)
        {
            if(dorms.m_campusType == DistrictPark.ParkType.University)
            {
                dorms.numApartments = 60;
            }
            else if(dorms.m_campusType == DistrictPark.ParkType.LiberalArts)
            {
                dorms.numApartments = 60;
            }
            else if(dorms.m_campusType == DistrictPark.ParkType.TradeSchool)
            {
                dorms.numApartments = 60;
            }

            return dorms;
        }

        

    }
}
