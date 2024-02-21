using CampusIndustriesHousingMod.AI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CampusIndustriesHousingMod.Utils 
{
    public static class AiReplacementHelper 
    {

        public static void ApplyNewAIToBuilding(BuildingInfo b, string type) 
        {
            try 
            {
                if (type == "Dorms")
                {
                    ChangeBuildingAI(b, typeof(DormsAI));
                }
                else if (type == "Barracks")
                {
                    ChangeBuildingAI(b, typeof(BarracksAI));
                }
                
                return;
            }
            catch (Exception e) 
            {
                Logger.LogInfo(e.ToString());
            }
        }

        private static void ChangeBuildingAI(BuildingInfo b, Type AIType) 
        {
            var oldAI = b.gameObject.GetComponent<BuildingAI>();
            var newAI = (BuildingAI)b.gameObject.AddComponent(AIType);
            TryCopyAttributes(oldAI, newAI, false);
            oldAI.DestroyPrefab();
            UnityEngine.Object.DestroyImmediate(oldAI);
            b.m_buildingAI = newAI;
            newAI.m_info = b;
            newAI.InitializePrefab();
        }

        private static void TryCopyAttributes(PrefabAI src, PrefabAI dst, bool safe = true) 
        {
            var oldAIFields = src.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            var newAIFields = dst.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            var newAIFieldDic = new Dictionary<string, FieldInfo>(newAIFields.Length);
            foreach (var field in newAIFields) 
            {
                newAIFieldDic.Add(field.Name, field);
            }

            foreach (var fieldInfo in oldAIFields) 
            {
                bool copyField = !fieldInfo.IsDefined(typeof(NonSerializedAttribute), true);

                if (safe && !fieldInfo.IsDefined(typeof(CustomizablePropertyAttribute), true)) copyField = false;

                if (copyField) 
                {
                    FieldInfo newAIField;
                    newAIFieldDic.TryGetValue(fieldInfo.Name, out newAIField);
                    try 
                    {
                        if (newAIField != null && newAIField.GetType().Equals(fieldInfo.GetType())) 
                        {
                            newAIField.SetValue(dst, fieldInfo.GetValue(src));
                        }
                    }
                    catch (NullReferenceException) {
                    }
                }
            }
        }
    }
}
