using System;
using System.Collections.Generic;

namespace CampusIndustriesHousingMod.Serializer
{
    public class StorageData
    {
        public static void WriteByte(byte Value, FastList<byte> Data) => Data.Add(Value);

        public static byte ReadByte(byte[] Data, ref int Index)
        {
            byte Byte = Data[Index];
            Index++;
            return Byte;
        }

        public static void WriteBool(bool Value, FastList<byte> Data) => AddToData(BitConverter.GetBytes(Value), Data);

        public static bool ReadBool(byte[] Data, ref int Index)
        {
            bool Boolean = BitConverter.ToBoolean(Data, Index);
            Index++;
            return Boolean;
        }

        public static void WriteUInt16(ushort Value, FastList<byte> Data) => AddToData(BitConverter.GetBytes(Value), Data);

        public static ushort ReadUInt16(byte[] Data, ref int Index)
        {
            ushort UInt16 = BitConverter.ToUInt16(Data, Index);
            Index += 2;
            return UInt16;
        }

        public static void WriteInt32(int Value, FastList<byte> Data) => AddToData(BitConverter.GetBytes(Value), Data);

        public static int ReadInt32(byte[] Data, ref int Index)
        {
            int Int32 = BitConverter.ToInt32(Data, Index);
            Index += 4;
            return Int32;
        }

        public static void WriteUInt32(uint Value, FastList<byte> Data) => AddToData(BitConverter.GetBytes(Value), Data);

        public static uint ReadUInt32(byte[] Data, ref int Index)
        {
            uint UInt32 = BitConverter.ToUInt32(Data, Index);
            Index += 4;
            return UInt32;
        }

        public static void WriteFloat(float Value, FastList<byte> Data) => AddToData(BitConverter.GetBytes(Value), Data);

        public static float ReadFloat(byte[] Data, ref int Index)
        {
            float FloatValue = BitConverter.ToSingle(Data, Index);
            Index += 4;
            return FloatValue;
        }

        public static void WriteInt32ArrayWithoutLength(int[] Int32Array, FastList<byte> Data)
        {
            for (int i = 0; i != Int32Array.Length; i++)
            {
                WriteInt32(Int32Array[i], Data);
            }
        }

        public static int[] ReadInt32ArrayWithoutLength(byte[] Data, ref int Index, int ArrayLength)
        {
            int[] Int32Array = new int[ArrayLength];
            for (int i = 0; i < ArrayLength; i++)
            {
                Int32Array[i] = ReadInt32(Data, ref Index);
            }
            return Int32Array;
        }

        public static void WriteUInt32ArrayWithoutLength(uint[] UInt32Array, FastList<byte> Data)
        {
            for (int i = 0; i != UInt32Array.Length; i++)
            {
                WriteUInt32(UInt32Array[i], Data);
            }
        }

        public static uint[] ReadUInt32ArrayWithoutLength(byte[] Data, ref int Index, int ArrayLength)
        {
            uint[] UInt32Array = new uint[ArrayLength];
            for (int i = 0; i < ArrayLength; i++)
            {
                UInt32Array[i] = ReadUInt32(Data, ref Index);
            }
            return UInt32Array;
        }

        public static void WriteUInt16TwoDimensionalArrayWithoutLength(ushort[,] UInt16Array, FastList<byte> Data)
        {
            for (int i = 0; i != UInt16Array.GetLength(0); i++)
            {
                for (int j = 0; j != UInt16Array.GetLength(1); j++)
                {
                    WriteUInt16(UInt16Array[i, j], Data);
                }
            }
        }

        public static ushort[,] ReadUInt16TwoDimensionalArrayWithoutLength(byte[] Data, ref int Index, int ArrayNumberOfLines, int ArrayNumberOfColumns)
        {
            ushort[,] UInt16Array = new ushort[ArrayNumberOfLines, ArrayNumberOfColumns];
            for (int i = 0; i != UInt16Array.GetLength(0); i++)
            {
                for (int j = 0; j != UInt16Array.GetLength(1); j++)
                {
                    UInt16Array[i, j] = ReadUInt16(Data, ref Index);
                }
            }
            return UInt16Array;
        }

        public static void WriteUInt16ArrayWithoutLength(ushort[] UInt16Array, FastList<byte> Data)
        {
            for (int i = 0; i != UInt16Array.GetLength(0); i++)
            {
                WriteUInt16(UInt16Array[i], Data);
            }
        }

        public static ushort[] ReadUInt16ArrayWithoutLength(byte[] Data, ref int Index, int ArrayLength)
        {
            ushort[] UInt16Array = new ushort[ArrayLength];
            for (int i = 0; i != UInt16Array.GetLength(0); i++)
            {
                UInt16Array[i] = ReadUInt16(Data, ref Index);
            }
            return UInt16Array;
        }

        public static void AddToData(byte[] Bytes, FastList<byte> Data)
        {
            for (int i = 0; i < Bytes.Length; i++)
            {
                byte item = Bytes[i];
                Data.Add(item);
            }
        }

        public static void WriteUShortList(List<ushort> listUInt16, FastList<byte> Data)
        {
            // Write out list
            WriteInt32(listUInt16.Count, Data);
            foreach (ushort value in listUInt16)
            {
                WriteUInt16(value, Data);
            }
        }

        public static void WriteUIntList(List<uint> listUInt32, FastList<byte> Data)
        {
            // Write out list
            WriteInt32(listUInt32.Count, Data);
            foreach (uint value in listUInt32)
            {
                WriteUInt32(value, Data);
            }
        }

        public static List<ushort> ReadUShortList(byte[] Data, ref int iIndex)
        {
            var list = new List<ushort>();
            if (Data.Length > iIndex + 4)
            {
                int iArrayCount = ReadInt32(Data, ref iIndex);
                if (Data.Length >= iIndex + iArrayCount * 2)
                {
                    for (int i = 0; i < iArrayCount; i++)
                    {
                        list.Add(ReadUInt16(Data, ref iIndex));
                    }
                }
                else
                {
                    Logger.LogError(Logger.LOG_SERIALIZATION, "Data size not large enough aborting read. ArraySize: " + iArrayCount + " DataSize: " + Data.Length + " Index: " + iIndex);
                }
            }
            return list;
        }

        public static List<uint> ReadUIntList(byte[] Data, ref int iIndex)
        {
            var list = new List<uint>();
            if (Data.Length > iIndex + 4)
            {
                int iArrayCount = ReadInt32(Data, ref iIndex);
                if (Data.Length >= iIndex + iArrayCount * 2)
                {
                    for (int i = 0; i < iArrayCount; i++)
                    {
                        list.Add(ReadUInt32(Data, ref iIndex));
                    }
                }
                else
                {
                    Logger.LogError(Logger.LOG_SERIALIZATION, "Data size not large enough aborting read. ArraySize: " + iArrayCount + " DataSize: " + Data.Length + " Index: " + iIndex);
                }
            }
            return list;
        }

        public static void WriteString(string s, FastList<byte> data)
        {
            char[] charArray = s.ToCharArray();
            WriteInt32(charArray.Length, data);
            for (ushort index = 0; (int)index < charArray.Length; ++index)
            {
                AddToData(BitConverter.GetBytes(charArray[(int)index]), data);
            }
        }

        public static string ReadString(byte[] data, ref int index)
        {
            string empty = string.Empty;
            int num = ReadInt32(data, ref index);
            for (int index1 = 0; index1 < num; ++index1)
            {
                empty += BitConverter.ToChar(data, index).ToString();
                index += 2;
            }
            return empty;
        }

        public static void WriteDateTime(DateTime Value, FastList<byte> Data) => AddToData(BitConverter.GetBytes(Value.Ticks), Data);

        public static DateTime ReadDateTime(byte[] Data, ref int Index)
        {
            var myDateTime = DateTime.FromBinary(BitConverter.ToInt64(Data, Index));
            return myDateTime;
        }
    }
}
