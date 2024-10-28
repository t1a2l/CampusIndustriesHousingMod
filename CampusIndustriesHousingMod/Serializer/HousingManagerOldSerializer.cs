using CampusIndustriesHousingMod.Managers;
using System.IO;

namespace CampusIndustriesHousingMod.Serializer
{
    public static class HousingManagerOldSerializer
    {
        /// <summary>
        /// Deserializes old savegame data.
        /// </summary>
        /// <param name="reader">Reader to deserialize from.</param>
        internal static void Deserialize(BinaryReader reader)
        {
            Logger.LogInfo(Logger.LOG_SERIALIZATION, "Deserializing building old data");

            reader.ReadInt32(); // read version

            // Clear dictionary.
            HousingManager.BuildingRecords.Clear();

            HousingManager.PrefabRecords.Clear();

            // Iterate through each entry read.
            int BuildingRecords_Length = reader.ReadInt32();

            Logger.LogInfo(Logger.LOG_SERIALIZATION, "BuildingRecords_Length: ", BuildingRecords_Length);

            for (int i = 0; i < BuildingRecords_Length; ++i)
            {
                // Dictionary entry key.
                uint buildingID = reader.ReadUInt32();

                HousingManager.BuildingRecord buildingRecord = new()
                {
                    BuildingAI = reader.ReadString(),
                    NumOfApartments = reader.ReadInt32(),
                    IsDefault = reader.ReadBoolean()
                };

                // Drop any empty entries.
                if (buildingRecord.BuildingAI == null)
                {
                    Logger.LogInfo(Logger.LOG_SERIALIZATION, "dropping empty entry for building ", buildingID);
                    continue;
                }

                // Add completed entry to dictionary.
                if (!HousingManager.BuildingRecordExist((ushort)buildingID))
                {
                    HousingManager.BuildingRecords.Add((ushort)buildingID, buildingRecord);
                    Logger.LogInfo(Logger.LOG_SERIALIZATION, "read entry for building ", buildingID);
                }
                else
                {
                    Logger.LogError(Logger.LOG_SERIALIZATION, "duplicate buildingRecord key for building ", buildingID);
                }
            }

            int PrefabRecords_Length = reader.ReadInt32();

            Logger.LogInfo(Logger.LOG_SERIALIZATION, "PrefabRecords_Length: ", PrefabRecords_Length);

            for (int i = 0; i < PrefabRecords_Length; ++i)
            {
                HousingManager.PrefabRecord prefabRecord = new()
                {
                    InfoName = reader.ReadString(),
                    BuildingAI = reader.ReadString(),
                    NumOfApartments = reader.ReadInt32()
                };

                // Drop any empty entries.
                if (prefabRecord.InfoName == null && prefabRecord.BuildingAI == null)
                {
                    Logger.LogInfo(Logger.LOG_SERIALIZATION, "dropping empty entry for prefab ", prefabRecord.InfoName);
                    continue;
                }

                // Add completed entry to list.
                if (!HousingManager.PrefabExist(prefabRecord.InfoName, prefabRecord.BuildingAI))
                {
                    HousingManager.PrefabRecords.Add(prefabRecord);
                    Logger.LogInfo(Logger.LOG_SERIALIZATION, "read entry for prefab ", prefabRecord.InfoName);
                }
                else
                {
                    Logger.LogError(Logger.LOG_SERIALIZATION, "duplicate prefabgRecord setting for prefab ", prefabRecord.InfoName);
                }
            }
        }
    }
}
