using System.Collections.Generic;
using System.IO;

namespace CampusIndustriesHousingMod
{
    public class HousingManager
    {
        internal static readonly Dictionary<uint, BuildingRecord> BuildingRecords = new Dictionary<uint, BuildingRecord>();

        internal static readonly List<PrefabRecord> PrefabRecords = new List<PrefabRecord>();

        private const bool LOG_SERIALIZATION = false;

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

        /// <summary>
        /// Serializes savegame data.
        /// </summary>
        /// <param name="writer">Binary writer instance to serialize to.</param>
        internal static void Serialize(BinaryWriter writer)
        {
            Logger.logInfo("serializing building data");

            // Write length of dictionary.
            writer.Write(BuildingRecords.Count);

            // Serialise each building entry.
            foreach (KeyValuePair<uint, BuildingRecord> entry in BuildingRecords)
            {
                // Local reference.
                BuildingRecord buildingRecord = entry.Value;

                // Serialize key and simple fields.
                writer.Write(entry.Key);

                writer.Write(buildingRecord.BuildingAI);

                writer.Write(buildingRecord.NumOfApartments);

                writer.Write(buildingRecord.WorkPlaceCount0);

                writer.Write(buildingRecord.WorkPlaceCount1);

                writer.Write(buildingRecord.WorkPlaceCount2);

                writer.Write(buildingRecord.WorkPlaceCount3);

                Logger.logInfo(LOG_SERIALIZATION, "wrote entry ", entry.Key);
            }

            // Write length of list.
            writer.Write(PrefabRecords.Count);

             // Serialise each prefab entry.
            foreach (PrefabRecord prefabRecord in PrefabRecords)
            {
                writer.Write(prefabRecord.Name);

                writer.Write(prefabRecord.BuildingAI);

                writer.Write(prefabRecord.NumOfApartments);

                writer.Write(prefabRecord.WorkPlaceCount0);

                writer.Write(prefabRecord.WorkPlaceCount1);

                writer.Write(prefabRecord.WorkPlaceCount2);

                writer.Write(prefabRecord.WorkPlaceCount3);

                Logger.logInfo(LOG_SERIALIZATION, "wrote entry ", prefabRecord.Name);
            }
        }

        /// <summary>
        /// Deserializes savegame data.
        /// </summary>
        /// <param name="reader">Reader to deserialize from.</param>
        internal static void Deserialize(BinaryReader reader)
        {
            Logger.logInfo("deserializing building data");

            // Clear dictionary.
            BuildingRecords.Clear();

            PrefabRecords.Clear();

            // Iterate through each entry read.
            int BuildingRecords_Lenght = reader.ReadInt32();
            for (int i = 0; i < BuildingRecords_Lenght; ++i)
            {
                // Dictionary entry key.
                uint buildingID = reader.ReadUInt32();

                BuildingRecord buildingRecord = new BuildingRecord
                {
                    BuildingAI = reader.ReadString(),
                    NumOfApartments = reader.ReadInt32(),
                    WorkPlaceCount0 = reader.ReadInt32(),
                    WorkPlaceCount1 = reader.ReadInt32(),
                    WorkPlaceCount2 = reader.ReadInt32(),
                    WorkPlaceCount3 = reader.ReadInt32(),
                };

                // Drop any empty entries.
                if (buildingRecord.BuildingAI == null)
                {
                    Logger.logInfo(LOG_SERIALIZATION, "dropping empty entry for building ", buildingID);
                    continue;
                }

                // Add completed entry to dictionary.
                if (!BuildingRecords.ContainsKey(buildingID))
                {
                    BuildingRecords.Add(buildingID, buildingRecord);
                    Logger.logInfo(LOG_SERIALIZATION, "read entry for building ", buildingID);
                }
                else
                {
                    Logger.logError(LOG_SERIALIZATION, "duplicate buildingRecord key for building ", buildingID);
                }
            }

            int PrefabRecords_Length = reader.ReadInt32();
            for (int i = 0; i < PrefabRecords_Length; ++i)
            {

                PrefabRecord prefabgRecord = new PrefabRecord
                {
                    Name =  reader.ReadString(),
                    BuildingAI = reader.ReadString(),
                    NumOfApartments = reader.ReadInt32(),
                    WorkPlaceCount0 = reader.ReadInt32(),
                    WorkPlaceCount1 = reader.ReadInt32(),
                    WorkPlaceCount2 = reader.ReadInt32(),
                    WorkPlaceCount3 = reader.ReadInt32(),
                };

                // Drop any empty entries.
                if (prefabgRecord.Name == null && prefabgRecord.BuildingAI == null)
                {
                    Logger.logInfo(LOG_SERIALIZATION, "dropping empty entry for prefab ", prefabgRecord.Name);
                    continue;
                }

                // Add completed entry to list.
                if (!PrefabRecords.Contains(prefabgRecord))
                {
                    PrefabRecords.Add(prefabgRecord);
                    Logger.logInfo(LOG_SERIALIZATION, "read entry for prefab ", prefabgRecord.Name);
                }
                else
                {
                    Logger.logError(LOG_SERIALIZATION, "duplicate prefabgRecord setting for prefab ", prefabgRecord.Name);
                }
            }
        }

    }
}
