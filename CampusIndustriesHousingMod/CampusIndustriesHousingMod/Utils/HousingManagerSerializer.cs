using System.Collections.Generic;
using System.IO;

namespace CampusIndustriesHousingMod.Utils 
{

	public static class HousingManagerSerializer 
	{
        private static bool LOG_SERIALIZATION = false;

		/// <summary>
        /// Serializes savegame data.
        /// </summary>
        /// <param name="writer">Binary writer instance to serialize to.</param>
        internal static void Serialize(BinaryWriter writer)
        {
            Logger.logInfo("serializing building data");

            // Write length of dictionary.
            writer.Write(HousingManager.BuildingRecords.Count);

            // Serialise each building entry.
            foreach (KeyValuePair<uint, HousingManager.BuildingRecord> entry in HousingManager.BuildingRecords)
            {
                // Local reference.
                HousingManager.BuildingRecord buildingRecord = entry.Value;

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
            writer.Write(HousingManager.PrefabRecords.Count);

             // Serialise each prefab entry.
            foreach (HousingManager.PrefabRecord prefabRecord in HousingManager.PrefabRecords)
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
            HousingManager.BuildingRecords.Clear();

            HousingManager.PrefabRecords.Clear();

            // Iterate through each entry read.
            int BuildingRecords_Lenght = reader.ReadInt32();
            for (int i = 0; i < BuildingRecords_Lenght; ++i)
            {
                // Dictionary entry key.
                uint buildingID = reader.ReadUInt32();

                HousingManager.BuildingRecord buildingRecord = new HousingManager.BuildingRecord
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
                if (!HousingManager.BuildingRecords.ContainsKey(buildingID))
                {
                    HousingManager.BuildingRecords.Add(buildingID, buildingRecord);
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

                HousingManager.PrefabRecord prefabgRecord = new HousingManager.PrefabRecord
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
                if (!HousingManager.PrefabRecords.Contains(prefabgRecord))
                {
                    HousingManager.PrefabRecords.Add(prefabgRecord);
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
