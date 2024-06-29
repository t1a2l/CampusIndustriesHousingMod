using System;
using HarmonyLib;
using CampusIndustriesHousingMod.AI;
using CampusIndustriesHousingMod.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;

namespace CampusIndustriesHousingMod.Patches
{
    [HarmonyPatch(typeof(BuildingInfo), "InitializePrefab")]
    public static class InitializePrefabBuildingPatch
    {
        private static readonly string[] BarracksNames = [
            "Barracks",
            "Residential",
            "Caravan",
            "Unterkünfte",
            "Landwohnheim",
            "Housing",
            "Houses"
        ];

        private static readonly string[] DormsNames = [
            "Dormitory",
            "Dorm",
            "Housing",
            "Dorms"
       ];

        public static void Prefix(BuildingInfo __instance)
        {
            try
            {
                if (__instance.m_class.m_service == ItemClass.Service.PlayerIndustry && BarracksNames.Any(s => __instance.name.Contains(s)) && __instance.GetAI() is not BarracksAI)
                {
                    var oldAI = __instance.GetComponent<PrefabAI>();
                    Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<BarracksAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                    if(newAI is BarracksAI barracksAI)
                    {
                        barracksAI.m_noiseAccumulation = 0;
                        barracksAI.m_noiseRadius = 0;
                    }
                } 
                else if (__instance.m_class.m_service == ItemClass.Service.PlayerEducation && DormsNames.Any(s => __instance.name.Contains(s)) && __instance.GetAI() is not DormsAI)
                {
                    var oldAI = __instance.GetComponent<PrefabAI>();
                    Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<DormsAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void Postfix()
        {
            try
            {
                BuildingInfo universityDormitoryBuildingInfo = PrefabCollection<BuildingInfo>.FindLoaded("University Dormitory 01");
                BuildingInfo farmWorkersBarracksBuildingInfo = PrefabCollection<BuildingInfo>.FindLoaded("Farm Workers Barracks 01");

                if(universityDormitoryBuildingInfo != null && farmWorkersBarracksBuildingInfo != null)
                {
                    Debug.Log("FarmFound");
                    float universityDormitoryCapcityModifier = Mod.getInstance().getOptionsManager().getDormsCapacityModifier();
                    float farmWorkersBarracksCapcityModifier = Mod.getInstance().getOptionsManager().getBarracksCapacityModifier();

                    uint index = 0U;
                    for (; PrefabCollection<BuildingInfo>.LoadedCount() > index; ++index)
                    {
                        BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded(index);

                        // Check for replacement of AI
                        if (buildingInfo != null && buildingInfo.GetAI() is DormsAI dormsAI)
                        {
                            buildingInfo.m_class = universityDormitoryBuildingInfo.m_class;
                            dormsAI.updateCapacity(universityDormitoryCapcityModifier);
                        }
                        else if (buildingInfo != null && buildingInfo.GetAI() is BarracksAI barracksAI)
                        {
                            buildingInfo.m_class = farmWorkersBarracksBuildingInfo.m_class;
                            barracksAI.updateCapacity(farmWorkersBarracksCapcityModifier);
                        }
                    }
                }                
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}