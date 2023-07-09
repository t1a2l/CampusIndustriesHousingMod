using CampusIndustriesHousingMod.AI;
using System.Collections.Generic;

namespace CampusIndustriesHousingMod.Utils
{
    public static class HousingManager
    {
        internal static Dictionary<uint, BuildingRecord> BuildingRecords;

        internal static List<PrefabRecord> PrefabRecords;

        public struct BuildingRecord
        {
            public string BuildingAI;

            public int NumOfApartments;

            public int WorkPlaceCount0;

            public int WorkPlaceCount1;

            public int WorkPlaceCount2;

            public int WorkPlaceCount3;
        }

        public struct PrefabRecord
        {
            public string Name;

            public string BuildingAI;

            public int NumOfApartments;

            public int WorkPlaceCount0;

            public int WorkPlaceCount1;

            public int WorkPlaceCount2;

            public int WorkPlaceCount3;
        }

        public static void Init()
        {
			BuildingRecords ??= new();
			PrefabRecords ??= new();
        }

        public static void Deinit()
        {
            BuildingRecords = new();
            PrefabRecords = new();
        }

        public static void AddBuilding(ushort buildingID, BuildingRecord newBuildingRecord)
        {
            // See if we've already got an entry for this building; if not, create one.
            if (!BuildingRecords.TryGetValue(buildingID, out _))
            {
                // Init buildingRecord.
                BuildingRecords.Add(buildingID, newBuildingRecord);
            }
            else
            {
               BuildingRecords[buildingID] = newBuildingRecord;
            }
        }

        public static void RemoveBuilding(ushort buildingID)
        {
            // See if we've already got an entry for this building; if not, create one.
            if (BuildingRecords.TryGetValue(buildingID, out _))
            {
                // Init buildingRecord.
                BuildingRecords.Remove(buildingID);
            }
        }

        public static void AddPrefab(PrefabRecord newPrefabRecord)
        {
            if (!PrefabRecords.Contains(newPrefabRecord))
            {
                PrefabRecords.Add(newPrefabRecord);
            }
            else
            {
               var index = PrefabRecords.FindIndex(x => x.Name == newPrefabRecord.Name);
               PrefabRecords[index] = newPrefabRecord;
            }
        }

        public static void RemovePrefab(PrefabRecord newPrefabRecord)
        {
            if (PrefabRecords.Contains(newPrefabRecord))
            {
                PrefabRecords.Remove(newPrefabRecord);
            }
        }

        public static BarracksAI DefaultBarracksValues(BarracksAI barracks)
        {
            if(barracks.m_industryType == DistrictPark.ParkType.Farming)
            {
                barracks.numApartments = 2;
                barracks.m_workPlaceCount0 = 5;
                barracks.m_workPlaceCount1 = 0;
                barracks.m_workPlaceCount2 = 0;
                barracks.m_workPlaceCount3 = 0;
            }
            else if(barracks.m_industryType == DistrictPark.ParkType.Forestry)
            {
                barracks.numApartments = 10;
                barracks.m_workPlaceCount0 = 5;
                barracks.m_workPlaceCount1 = 2;
                barracks.m_workPlaceCount2 = 0;
                barracks.m_workPlaceCount3 = 0;
            }
            else if(barracks.m_industryType == DistrictPark.ParkType.Oil)
            {
                barracks.numApartments = 50;
                barracks.m_workPlaceCount0 = 5;
                barracks.m_workPlaceCount1 = 2;
                barracks.m_workPlaceCount2 = 0;
                barracks.m_workPlaceCount3 = 0;
            }
            else if(barracks.m_industryType == DistrictPark.ParkType.Ore)
            {
                barracks.numApartments = 48;
                barracks.m_workPlaceCount0 = 5;
                barracks.m_workPlaceCount1 = 2;
                barracks.m_workPlaceCount2 = 0;
                barracks.m_workPlaceCount3 = 0;
            }

            return barracks;
        }

        public static DormsAI DefaultDormsValues(DormsAI dorms)
        {
            if(dorms.m_campusType == DistrictPark.ParkType.University)
            {
                dorms.numApartments = 60;
                dorms.m_workPlaceCount0 = 3;
                dorms.m_workPlaceCount1 = 3;
                dorms.m_workPlaceCount2 = 0;
                dorms.m_workPlaceCount3 = 0;
            }
            else if(dorms.m_campusType == DistrictPark.ParkType.LiberalArts)
            {
                dorms.numApartments = 60;
                dorms.m_workPlaceCount0 = 3;
                dorms.m_workPlaceCount1 = 3;
                dorms.m_workPlaceCount2 = 0;
                dorms.m_workPlaceCount3 = 0;
            }
            else if(dorms.m_campusType == DistrictPark.ParkType.TradeSchool)
            {
                dorms.numApartments = 60;
                dorms.m_workPlaceCount0 = 3;
                dorms.m_workPlaceCount1 = 3;
                dorms.m_workPlaceCount2 = 0;
                dorms.m_workPlaceCount3 = 0;
            }

            return dorms;
        }

        

    }
}
