using System;
using CampusIndustriesHousingMod.Managers;

namespace CampusIndustriesHousingMod.Serializer
{
    public class HousingSerializer
    {
        // Some magic values to check we are line up correctly on the tuple boundaries
        private const uint uiTUPLE_START = 0xFEFEFEFE;
        private const uint uiTUPLE_END = 0xFAFAFAFA;

        private const ushort iHOUSING_DATA_VERSION = 4;

        public static void SaveData(FastList<byte> Data)
        {
            StorageData.WriteUInt16(iHOUSING_DATA_VERSION, Data);

            Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData iHOUSING_DATA_VERSION: " + iHOUSING_DATA_VERSION);

            StorageData.WriteUInt32(uiTUPLE_START, Data);

            Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData uiTUPLE_START: " + uiTUPLE_START);

            StorageData.WriteInt32(HousingManager.BuildingRecords.Count, Data);

            Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData BuildingRecords_Count: " + HousingManager.BuildingRecords.Count);

            // Serialize each building record for housing
            foreach (var kvp in HousingManager.BuildingRecords)
            {
                // Write start tuple
                StorageData.WriteUInt32(uiTUPLE_START, Data);

                Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData uiTUPLE_START: " + uiTUPLE_START);

                // Write actual settings
                StorageData.WriteUInt16(kvp.Key, Data);
                StorageData.WriteString(kvp.Value.BuildingAI, Data);
                StorageData.WriteInt32(kvp.Value.NumOfApartments, Data);
                StorageData.WriteBool(kvp.Value.IsDefault, Data);
                StorageData.WriteBool(kvp.Value.IsPrefab, Data);
                StorageData.WriteBool(kvp.Value.IsGlobal, Data);
                StorageData.WriteBool(kvp.Value.IsLocked, Data);

                Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData BuildingId: " + kvp.Key);
                Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData BuildingAI: " + kvp.Value.BuildingAI);
                Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData NumOfApartments: " + kvp.Value.NumOfApartments);
                Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData IsDefault: " + kvp.Value.IsDefault);
                Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData IsPrefab: " + kvp.Value.IsPrefab);
                Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData IsGlobal: " + kvp.Value.IsGlobal);
                Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData IsLocked: " + kvp.Value.IsLocked);

                // Write end tuple
                StorageData.WriteUInt32(uiTUPLE_END, Data);

                Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData uiTUPLE_END: " + uiTUPLE_END);
            }

            StorageData.WriteUInt32(uiTUPLE_END, Data);

            Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData uiTUPLE_END: " + uiTUPLE_END);

            StorageData.WriteUInt32(uiTUPLE_START, Data);

            Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData uiTUPLE_START: " + uiTUPLE_START);

            StorageData.WriteInt32(HousingManager.PrefabRecords.Count, Data);

            Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData PrefabRecord_Count: " + HousingManager.PrefabRecords.Count);

            // Serialize each prefab entry.
            foreach (var prefabRecord in HousingManager.PrefabRecords)
            {
                // Write start tuple
                StorageData.WriteUInt32(uiTUPLE_START, Data);

                Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData uiTUPLE_START: " + uiTUPLE_START);

                StorageData.WriteString(prefabRecord.InfoName, Data);
                StorageData.WriteString(prefabRecord.BuildingAI, Data);
                StorageData.WriteInt32(prefabRecord.NumOfApartments, Data);

                Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData Prefab InfoName: " + prefabRecord.InfoName);
                Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData Prefab BuildingAI: " + prefabRecord.BuildingAI);
                Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData Prefab NumOfApartments: " + prefabRecord.NumOfApartments);

                // Write end tuple
                StorageData.WriteUInt32(uiTUPLE_END, Data);

                Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData uiTUPLE_END: " + uiTUPLE_END);
            }

            StorageData.WriteUInt32(uiTUPLE_END, Data);

            Logger.LogInfo(Logger.LOG_SERIALIZATION, "SaveData uiTUPLE_END: " + uiTUPLE_END);
        }

        public static void LoadData(int iGlobalVersion, byte[] Data, ref int iIndex)
        {
            if (Data != null && Data.Length > iIndex)
            {
                HousingManager.BuildingRecords ??= [];

                HousingManager.PrefabRecords ??= [];

                if (HousingManager.BuildingRecords.Count > 0)
                {
                    HousingManager.BuildingRecords.Clear();
                }

                if (HousingManager.PrefabRecords.Count > 0)
                {
                    HousingManager.PrefabRecords.Clear();
                }

                int iHousingVersion = StorageData.ReadUInt16(Data, ref iIndex);

                Logger.LogInfo(Logger.LOG_SERIALIZATION, "Global: " + iGlobalVersion + " BufferVersion: " + iHousingVersion + " DataLength: " + Data.Length + " Index: " + iIndex);

                CheckStartTuple($"BuildingRecords Start", iHousingVersion, Data, ref iIndex);

                int BuildingRecords_Count = StorageData.ReadInt32(Data, ref iIndex);

                Logger.LogInfo(Logger.LOG_SERIALIZATION, "LoadData BuildingRecords_Count: " + BuildingRecords_Count);

                for (int i = 0; i < BuildingRecords_Count; i++)
                {
                    CheckStartTuple($"Buffer({i})", iHousingVersion, Data, ref iIndex);

                    ushort BuildingId = StorageData.ReadUInt16(Data, ref iIndex);

                    string BuildingAI = StorageData.ReadString(Data, ref iIndex);
                    int NumOfApartments = StorageData.ReadInt32(Data, ref iIndex);
                    bool IsDefault = StorageData.ReadBool(Data, ref iIndex);

                    Logger.LogInfo(Logger.LOG_SERIALIZATION, "LoadData BuildingId: " + BuildingId);
                    Logger.LogInfo(Logger.LOG_SERIALIZATION, "LoadData BuildingAI: " + BuildingAI);
                    Logger.LogInfo(Logger.LOG_SERIALIZATION, "LoadData NumOfApartments: " + NumOfApartments);
                    Logger.LogInfo(Logger.LOG_SERIALIZATION, "LoadData IsDefault: " + IsDefault);

                    var builidngRecord = new HousingManager.BuildingRecord()
                    {
                        BuildingAI = BuildingAI,
                        NumOfApartments = NumOfApartments,
                        IsDefault = IsDefault,
                        IsPrefab = false,
                        IsGlobal = false,
                        IsLocked = false
                    };

                    if (iHousingVersion > 2)
                    {
                        builidngRecord.IsPrefab = StorageData.ReadBool(Data, ref iIndex);
                        builidngRecord.IsGlobal = StorageData.ReadBool(Data, ref iIndex);

                        Logger.LogInfo(Logger.LOG_SERIALIZATION, "LoadData IsPrefab: " + builidngRecord.IsPrefab);
                        Logger.LogInfo(Logger.LOG_SERIALIZATION, "LoadData IsGlobal: " + builidngRecord.IsGlobal);
                    }

                    if (iHousingVersion > 3)
                    {
                        builidngRecord.IsLocked = StorageData.ReadBool(Data, ref iIndex);

                        Logger.LogInfo(Logger.LOG_SERIALIZATION, "LoadData IsLocked: " + builidngRecord.IsLocked);
                    }

                    HousingManager.BuildingRecords.Add(BuildingId, builidngRecord);

                    CheckEndTuple($"Buffer({i})", iHousingVersion, Data, ref iIndex);
                }

                CheckEndTuple($"BuildingRecords End", iHousingVersion, Data, ref iIndex);

                //--------------------------------------------------------------------------------------

                CheckStartTuple($"PrefabRecords Start", iHousingVersion, Data, ref iIndex);

                int PrefabRecords_Count = StorageData.ReadInt32(Data, ref iIndex);

                Logger.LogInfo(Logger.LOG_SERIALIZATION, "LoadData PrefabRecords_Count: " + PrefabRecords_Count);

                for (int i = 0; i < PrefabRecords_Count; i++)
                {
                    CheckStartTuple($"Buffer({i})", iHousingVersion, Data, ref iIndex);

                    string InfoName = StorageData.ReadString(Data, ref iIndex);
                    string BuildingAI = StorageData.ReadString(Data, ref iIndex);
                    int NumOfApartments = StorageData.ReadInt32(Data, ref iIndex);

                    Logger.LogInfo(Logger.LOG_SERIALIZATION, "LoadData Prefab InfoName: " + InfoName);
                    Logger.LogInfo(Logger.LOG_SERIALIZATION, "LoadData Prefab BuildingAI: " + BuildingAI);
                    Logger.LogInfo(Logger.LOG_SERIALIZATION, "LoadData Prefab NumOfApartments: " + NumOfApartments);

                    var housing = new HousingManager.PrefabRecord()
                    {
                        InfoName = InfoName,
                        BuildingAI = BuildingAI,
                        NumOfApartments = NumOfApartments
                    };

                    HousingManager.PrefabRecords.Add(housing);

                    CheckEndTuple($"Buffer({i})", iHousingVersion, Data, ref iIndex);
                }

                CheckEndTuple($"PrefabRecords End", iHousingVersion, Data, ref iIndex);
                
            }
        }

        private static void CheckStartTuple(string sTupleLocation, int iDataVersion, byte[] Data, ref int iIndex)
        {
            if (iDataVersion >= 1)
            {
                uint iTupleStart = StorageData.ReadUInt32(Data, ref iIndex);
                Logger.LogInfo(Logger.LOG_SERIALIZATION, "LoadData iTupleStart: " + iTupleStart);

                if (iTupleStart != uiTUPLE_START)
                {
                    throw new Exception($"Housing Buffer start tuple not found at: {sTupleLocation}");
                }
            }
        }

        private static void CheckEndTuple(string sTupleLocation, int iDataVersion, byte[] Data, ref int iIndex)
        {
            if (iDataVersion >= 1)
            {
                uint iTupleEnd = StorageData.ReadUInt32(Data, ref iIndex);
                Logger.LogInfo(Logger.LOG_SERIALIZATION, "LoadData iTupleEnd: " + iTupleEnd);

                if (iTupleEnd != uiTUPLE_END)
                {
                    throw new Exception($"Housing Buffer end tuple not found at: {sTupleLocation}");
                }
            }
        }

    }
}
