using System;
using System.Text;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using CampusIndustriesHousingMod.Utils;
using CampusIndustriesHousingMod.Managers;

namespace CampusIndustriesHousingMod.AI
{
    public class BarracksAI : AuxiliaryBuildingAI 
    {
        private Randomizer randomizer = new(97);

        [CustomizableProperty("Number of Apartments")]
        public int numApartments = 50;

        public override Color GetColor(ushort buildingId, ref Building data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode) 
        {
            // This is a copy from ResidentialBuildingAI
            switch (infoMode) 
            {
                case InfoManager.InfoMode.Health:
                    if (ShowConsumption(buildingId, ref data) && data.m_citizenCount != 0)
                    {
                        if (subInfoMode == InfoManager.SubInfoMode.WindPower && data.m_children + data.m_teens != 0)
                        {
                            return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_targetColor, (float)Citizen.GetHealthLevel(data.m_childHealth) * 0.2f);
                        }
                        if (subInfoMode == InfoManager.SubInfoMode.PipeWater && data.m_seniors != 0)
                        {
                            return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_targetColor, (float)Citizen.GetHealthLevel(data.m_seniorHealth) * 0.2f);
                        }
                        if (subInfoMode == InfoManager.SubInfoMode.Default || subInfoMode == InfoManager.SubInfoMode.WaterPower)
                        {
                            return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_targetColor, (float)Citizen.GetHealthLevel(data.m_health) * 0.2f);
                        }
                        return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                case InfoManager.InfoMode.Density:
                    if (ShowConsumption(buildingId, ref data) && data.m_citizenCount != 0)
			        {
				        int num4 = (data.m_citizenCount - data.m_youngs - data.m_adults - data.m_seniors) * 3;
				        int num5 = data.m_youngs + data.m_adults;
				        int seniors = data.m_seniors;
				        if (num4 == 0 && num5 == 0 && seniors == 0)
				        {
					        return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
				        }
				        if (num4 >= num5 && num4 >= seniors)
				        {
					        return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
				        }
				        if (num5 >= seniors)
				        {
					        return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColorB;
				        }
				        return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_negativeColor;
			        }
			        return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                case InfoManager.InfoMode.Happiness:
                    if (ShowConsumption(buildingId, ref data))
                    {
                        return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_targetColor, (float)Citizen.GetHappinessLevel((int)data.m_happiness) * 0.25f);
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                case InfoManager.InfoMode.Post:
                    if (ShowConsumption(buildingId, ref data))
                    {
                        int num = CalculateHomeCount((ItemClass.Level)data.m_level, new Randomizer(buildingId), data.Width, data.Length);
                        if (num != 0)
                        {
                            return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_targetColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_negativeColor, Mathf.Min(100, data.m_mailBuffer * 2 / num) * 0.01f);
                        }
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                default:
                    return base.GetColor(buildingId, ref data, infoMode, subInfoMode);
            }
        }

        private int CalculateHomeCount(ItemClass.Level level, Randomizer r, int width, int length)
        {
            int num = level switch
            {
                ItemClass.Level.Level1 => 60,
                ItemClass.Level.Level2 => 100,
                ItemClass.Level.Level3 => 130,
                ItemClass.Level.Level4 => 150,
                _ => 160,
            };
            return Mathf.Max(100, width * length * num + r.Int32(100u)) / 100;
        }

        public override void CreateBuilding(ushort buildingID, ref Building data)
        {
            base.CreateBuilding(buildingID, ref data);
            var buildingRecord = HousingManager.CreateBuildingRecord(buildingID);
            Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, buildingRecord.NumOfApartments);
            ValidateCapacity(buildingID, ref data, false);
        }

        public override void EndRelocating(ushort buildingID, ref Building data)
        {
            base.EndRelocating(buildingID, ref data);
            ValidateCapacity(buildingID, ref data, false);
            EnsureCitizenUnits(buildingID, ref data, GetModifiedCapacity(buildingID));
        }

        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData) 
        {
            base.SimulationStep(buildingID, ref buildingData, ref frameData);
        }

        protected override void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
	    {
			Citizen.BehaviourData behaviour = default;
			int aliveCount = 0;
			int totalCount = 0;
            int homeCount = 0;
            int aliveHomeCount = 0;
            int emptyHomeCount = 0;

            GetHomeBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveCount, ref totalCount, ref homeCount, ref aliveHomeCount, ref emptyHomeCount);

            DistrictManager districtManager = Singleton<DistrictManager>.instance;
            byte district = districtManager.GetDistrict(buildingData.m_position);
            DistrictPolicies.Services policies = districtManager.m_districts.m_buffer[district].m_servicePolicies;

            districtManager.m_districts.m_buffer[district].m_servicePoliciesEffect |= policies & (DistrictPolicies.Services.PowerSaving | DistrictPolicies.Services.WaterSaving | DistrictPolicies.Services.SmokeDetectors | DistrictPolicies.Services.PetBan | DistrictPolicies.Services.Recycling | DistrictPolicies.Services.SmokingBan | DistrictPolicies.Services.ExtraInsulation | DistrictPolicies.Services.NoElectricity | DistrictPolicies.Services.OnlyElectricity);

            GetConsumptionRates(new Randomizer(buildingID), 100, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out _);

            int modifiedElectricityConsumption = 1 + (electricityConsumption * behaviour.m_electricityConsumption + 9999) / 10000;
            waterConsumption = 1 + (waterConsumption * behaviour.m_waterConsumption + 9999) / 10000;
            int modifiedSewageAccumulation = 1 + (sewageAccumulation * behaviour.m_sewageAccumulation + 9999) / 10000;
            garbageAccumulation = (garbageAccumulation * behaviour.m_garbageAccumulation + 9999) / 10000;
            int modifiedIncomeAccumulation = 0;

            // Handle Heating
            int heatingConsumption = 0;
            if (modifiedElectricityConsumption != 0 && districtManager.IsPolicyLoaded(DistrictPolicies.Policies.ExtraInsulation)) 
            {
                if ((policies & DistrictPolicies.Services.ExtraInsulation) != DistrictPolicies.Services.None) 
                {
                    heatingConsumption = Mathf.Max(1, modifiedElectricityConsumption * 3 + 8 >> 4);
                } 
                else
                {
                    heatingConsumption = Mathf.Max(1, modifiedElectricityConsumption + 2 >> 2);
                }
            }

            // Handle Recylcing and Pets
            if (garbageAccumulation != 0) 
            {
                if ((policies & DistrictPolicies.Services.Recycling) != DistrictPolicies.Services.None) 
                {
                    garbageAccumulation = (policies & DistrictPolicies.Services.PetBan) == DistrictPolicies.Services.None ? Mathf.Max(1, garbageAccumulation * 85 / 100) : Mathf.Max(1, garbageAccumulation * 7650 / 10000);
                    modifiedIncomeAccumulation = modifiedIncomeAccumulation * 95 / 100;
                } 
                else if ((policies & DistrictPolicies.Services.PetBan) != DistrictPolicies.Services.None) 
                {
                    garbageAccumulation = Mathf.Max(1, garbageAccumulation * 90 / 100);
                }
            }

            if (buildingData.m_fireIntensity == 0) 
            {
                int maxMail = 100;
                int mailAccumulation = 1;
                HandleCommonConsumption(buildingID, ref buildingData, ref frameData, ref modifiedElectricityConsumption, ref heatingConsumption, ref waterConsumption, ref modifiedSewageAccumulation, ref garbageAccumulation, ref mailAccumulation, maxMail, policies);
                buildingData.m_flags |= Building.Flags.Active;
            } 
            else 
            {
                // Handle on fire
                modifiedElectricityConsumption = 0;
                heatingConsumption = 0;
                waterConsumption = 0;
                modifiedSewageAccumulation = 0;
                garbageAccumulation = 0;
                buildingData.m_problems = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem1.Electricity | Notification.Problem1.Water | Notification.Problem1.Sewage | Notification.Problem1.Flood | Notification.Problem1.Heating);
                buildingData.m_flags &= ~Building.Flags.Active;
            }


            buildingData.m_customBuffer1 = (ushort)aliveCount;
            int health = 0;
            float radius = (buildingData.Width + buildingData.Length) * 2.5f;
            if (behaviour.m_healthAccumulation != 0) 
            {
                if (aliveCount != 0) 
                {
                    health = (behaviour.m_healthAccumulation + (aliveCount >> 1)) / aliveCount;
                }
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.ElderCare, behaviour.m_healthAccumulation, buildingData.m_position, radius);
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Health, behaviour.m_healthAccumulation, buildingData.m_position, radius);
            }
            Logger.LogInfo(Logger.LOG_BARRACKS_SIMULATION, "BarracksAI.SimulationStepActive -- health: {0}", health);

            // Get the Wellbeing
            int wellbeing = 0;
            if (behaviour.m_wellbeingAccumulation != 0) 
            {
                if (aliveCount != 0) 
                {
                    wellbeing = (behaviour.m_wellbeingAccumulation + (aliveCount >> 1)) / aliveCount;
                }
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Wellbeing, behaviour.m_wellbeingAccumulation, buildingData.m_position, radius);
            }
            Logger.LogInfo(Logger.LOG_BARRACKS_SIMULATION, "BarracksAI.SimulationStepActive -- wellbeing: {0}", wellbeing);

            if (aliveCount != 0) 
            {
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Density, aliveCount, buildingData.m_position, radius);
            }

            // Calculate Happiness
            int happiness = Citizen.GetHappiness(health, wellbeing);
            if ((buildingData.m_problems & Notification.Problem1.MajorProblem) != Notification.Problem1.None) 
            {
                happiness -= happiness >> 1;
            } 
            else if (buildingData.m_problems != Notification.Problem1.None) 
            {
                happiness -= happiness >> 2;
            }
            Logger.LogInfo(Logger.LOG_BARRACKS_SIMULATION, "BarracksAI.SimulationStepActive -- happiness: {0}", happiness);

            buildingData.m_health = (byte) health;
            buildingData.m_happiness = (byte) happiness;
            buildingData.m_citizenCount = (byte) aliveCount;
            buildingData.m_education1 = (byte) behaviour.m_education1Count;
            buildingData.m_education2 = (byte) behaviour.m_education2Count;
            buildingData.m_education3 = (byte) behaviour.m_education3Count;
            buildingData.m_teens = (byte) behaviour.m_teenCount;
            buildingData.m_youngs = (byte) behaviour.m_youngCount;
            buildingData.m_adults = (byte) behaviour.m_adultCount;
            buildingData.m_seniors = (byte) behaviour.m_seniorCount;

            HandleSick(buildingID, ref buildingData, ref behaviour, totalCount);
            HandleDead2(buildingID, ref buildingData, ref behaviour, totalCount);

            // Handle Crime and Fire Factors
            int crimeAccumulation = behaviour.m_crimeAccumulation / (3 * GetModifiedCapacity(buildingID));
            if ((policies & DistrictPolicies.Services.RecreationalUse) != DistrictPolicies.Services.None) 
            {
                crimeAccumulation = crimeAccumulation * 3 + 3 >> 2;
            }
            HandleCrime(buildingID, ref buildingData, crimeAccumulation, aliveCount);
            int crimeBuffer = buildingData.m_crimeBuffer;
            int crimeRate;
            if (aliveCount != 0) 
            {
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Density, aliveCount, buildingData.m_position, radius);
                // num1
                int fireFactor = (behaviour.m_educated0Count * 30 + behaviour.m_educated1Count * 15 + behaviour.m_educated2Count * 10) / aliveCount + 50;
                if (buildingData.m_crimeBuffer > aliveCount * 40) 
                {
                    fireFactor += 30;
                } 
                else if (buildingData.m_crimeBuffer > aliveCount * 15) 
                {
                    fireFactor += 15;
                } 
                else if (buildingData.m_crimeBuffer > aliveCount * 5) 
                {
                    fireFactor += 10;
                }
                buildingData.m_fireHazard = (byte) fireFactor;
                crimeRate = (crimeBuffer + (aliveCount >> 1)) / aliveCount;
            } 
            else 
            {
                buildingData.m_fireHazard = (byte) 0;
                crimeRate = 0;
            }

            districtManager.m_districts.m_buffer[(int) district].AddResidentialData(ref behaviour, aliveCount, health, happiness, crimeRate, homeCount, aliveHomeCount, emptyHomeCount, (int) m_info.m_class.m_level, modifiedElectricityConsumption, heatingConsumption, waterConsumption, modifiedSewageAccumulation, garbageAccumulation, modifiedIncomeAccumulation, Mathf.Min(100, (int) buildingData.m_garbageBuffer / 50), (int) buildingData.m_waterPollution * 100 / (int) byte.MaxValue, m_info.m_class.m_subService);

            // Handle custom maintenance in addition to the standard maintenance handled in the base class
            HandleAdditionalMaintenanceCost(buildingID, ref buildingData);
		    
            base.SimulationStepActive(buildingID, ref buildingData, ref frameData);
            HandleFire(buildingID, ref buildingData, ref frameData, policies);
	    }
        
        public void ProduceGoodsPublic(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
        }

        protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
        {
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);    
            if (finalProductionRate == 0)
		    {
			    return;
		    }
            GetOccupancyDetails(ref buildingData, out int numResidents, out int numApartmentsOccupied);

            // Make sure there are no problems
            if ((buildingData.m_problems & (Notification.Problem1.MajorProblem | Notification.Problem1.Electricity | Notification.Problem1.ElectricityNotConnected | Notification.Problem1.Fire | Notification.Problem1.Water | Notification.Problem1.WaterNotConnected | Notification.Problem1.TurnedOff)) != Notification.Problem1.None) 
            {
                return;
            }

            WorkerManager workerManager = WorkerManager.GetInstance();
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;

            // Fetch a Worker and family that wants to move in 
            uint[] familyWithWorkers = workerManager.GetFamilyWithWorkers(buildingData);
            if (familyWithWorkers != null) 
            {
                // Make sure there are empty apartments available
                uint emptyApartment = GetEmptyCitizenUnit(ref buildingData);
                if (emptyApartment == 0) 
                {
                    return;
                }

                Logger.LogInfo(Logger.LOG_BARRACKS_PRODUCTION, "------------------------------------------------------------");
                Logger.LogInfo(Logger.LOG_BARRACKS_PRODUCTION, "BarracksAI.ProduceGoods -- Worker Family: {0}", string.Join(", ", Array.ConvertAll(familyWithWorkers, item => item.ToString())));
                // Check move in chance
                bool shouldMoveIn = MoveInProbabilityHelper.CheckIfShouldMoveIn(familyWithWorkers, ref buildingData, ref randomizer, "worker");

                // Process the worker and his family and move them in if able to, mark the worker as done processing regardless
                if (shouldMoveIn)
                {
                    Logger.LogInfo(Logger.LOG_BARRACKS_PRODUCTION, "BarracksAI.ProduceGoods -- shouldMoveIn");
                    foreach (uint familyMember in familyWithWorkers) 
                    {
                        if(familyMember != 0)
                        {
                            Logger.LogInfo(Logger.LOG_BARRACKS_PRODUCTION, "BarracksAI.ProduceGoods -- Moving In: {0}", familyMember);
                            citizenManager.m_citizens.m_buffer[familyMember].SetHome(familyMember, buildingID, emptyApartment);
                        }
                    }

                    workerManager.DoneProcessingFamily(familyWithWorkers);
                }
            }

            // Fetch a barracks family that needs to move out because none of the family members is working in this indastrial area no more
            uint[] BarracksApartmentFamily = workerManager.GetBarracksApartmentFamily(buildingData);
            if (BarracksApartmentFamily != null) 
            {
                Logger.LogInfo(Logger.LOG_BARRACKS_PRODUCTION, "------------------------------------------------------------");
                Logger.LogInfo(Logger.LOG_BARRACKS_PRODUCTION, "BarracksAI.ProduceGoods -- BarracksApartmentFamily: {0}", string.Join(", ", Array.ConvertAll(BarracksApartmentFamily, item => item.ToString())));

                foreach (uint familyMember in BarracksApartmentFamily) 
                {
                    if(familyMember != 0)
                    {
                        Logger.LogInfo(Logger.LOG_BARRACKS_PRODUCTION, "BarracksAI.ProduceGoods -- Moving Out: {0}", familyMember);
                        citizenManager.m_citizens.m_buffer[familyMember].SetHome(familyMember, 0, 0);
                    }
                }

                workerManager.DoneProcessingFamily(BarracksApartmentFamily);
            }

        }
        
        public override string GetLocalizedStats(ushort buildingID, ref Building data) 
        {
            GetOccupancyDetails(ref data, out int numResidents, out int numApartmentsOccupied);
            StringBuilder stringBuilder = new();
            DistrictManager instance = Singleton<DistrictManager>.instance;
		    byte b = instance.GetPark(data.m_position);
		    if (b != 0)
		    {
			    if (!instance.m_parks.m_buffer[b].IsIndustry)
			    {
				    b = 0;
			    }
			    else if (m_industryType == DistrictPark.ParkType.Industry || m_industryType != instance.m_parks.m_buffer[b].m_parkType)
			    {
				    b = 0;
			    }
		    }
            if (m_workEfficiencyDelta != 0 && b != 0)
		    {
			    if (stringBuilder.Length != 0)
			    {
				    stringBuilder.Append(Environment.NewLine);
			    }
                stringBuilder.Append(LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_WORK_EFFICIENCY", 100 + instance.m_parks.m_buffer[b].m_finalWorkEfficiencyDelta));
		    }
		    if (m_storageDelta != 0 && b != 0)
		    {
			    if (stringBuilder.Length != 0)
			    {
				    stringBuilder.Append(Environment.NewLine);
			    }
			    stringBuilder.Append(LocaleFormatter.FormatGeneric("AIINFO_INDUSTRY_STORAGE_EFFICIENCY", 100 + instance.m_parks.m_buffer[b].m_finalStorageDelta));
		    }
            if (stringBuilder.Length != 0)
			{
				stringBuilder.Append(Environment.NewLine);
			}
            stringBuilder.Append(string.Format("Apartments Occupied: {0} of {1}", numApartmentsOccupied, GetModifiedCapacity(buildingID)));
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(string.Format("Number of Residents: {0}", numResidents));
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(Environment.NewLine);
            return stringBuilder.ToString();
        }

        private int GetCustomMaintenanceCost(ushort buildingID, ref Building buildingData) 
        {
            int originalAmount = -(m_maintenanceCost * 100);

            Mod mod = Mod.GetInstance();
            if (mod == null) 
            {
                return 0;
            }

            OptionsManager optionsManager = mod.GetOptionsManager();
            if (optionsManager == null) 
            {
                return 0;
            }

            GetOccupancyDetails(ref buildingData, numResidents: out int _, out int numApartmentsOccupied);
            float capacityModifier = numApartmentsOccupied / (float)GetModifiedCapacity(buildingID);
            int modifiedAmount = (int) (originalAmount * capacityModifier);

            int amount = 0;
            switch (optionsManager.GetBarracksIncomeModifier()) 
            {
                case OptionsManager.IncomeValues.FULL_MAINTENANCE:
                    return 0;
                case OptionsManager.IncomeValues.HALF_MAINTENANCE:
                    amount = modifiedAmount / 2;
                    break;
                case OptionsManager.IncomeValues.NO_MAINTENANCE:
                    amount = modifiedAmount;
                    break;
                case OptionsManager.IncomeValues.NORMAL_PROFIT:
                    amount = modifiedAmount * 2;
                    break;
                case OptionsManager.IncomeValues.DOUBLE_DOUBLE:
                    amount = -originalAmount + (modifiedAmount * 4);
                    break;
                case OptionsManager.IncomeValues.DOUBLE_PROFIT:
                    amount = modifiedAmount * 3;
                    break;
            }

            if(amount == 0) 
            {
                return 0;
            }
            
            Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetMaintenanceCost(ref amount, m_info.m_class.m_service, m_info.m_class.m_subService, m_info.m_class.m_level);
            Logger.LogInfo(Logger.LOG_BARRACKS_INCOME, "GetCustomMaintenanceCost - building: {0} - calculated maintenance amount: {1}", buildingData.m_buildIndex, amount);

            return amount;
        }

        public void HandleAdditionalMaintenanceCost(ushort buildingID, ref Building buildingData) 
        {
            int amount = GetCustomMaintenanceCost(buildingID, ref buildingData);
            if (amount == 0) 
            {
                return;
            }

            int productionRate = (int) buildingData.m_productionRate;
            int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
            amount /= 100;
            amount = productionRate * budget / 100 * amount / 100;
            Logger.LogInfo(Logger.LOG_BARRACKS_INCOME, "GetCustomMaintenanceCost - building: {0} - adjusted maintenance amount: {1}", buildingData.m_buildIndex, amount);

            if ((buildingData.m_flags & Building.Flags.Original) == Building.Flags.None && amount != 0) 
            {
                Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, amount, m_info.m_class);
            }
        }

        private uint GetEmptyCitizenUnit(ref Building data) 
        {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            uint citizenUnitIndex = data.m_citizenUnits;

            while ((int) citizenUnitIndex != 0) 
            {
                uint nextCitizenUnitIndex = citizenManager.m_units.m_buffer[citizenUnitIndex].m_nextUnit;
                if ((citizenManager.m_units.m_buffer[citizenUnitIndex].m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None) 
                {
                    if (citizenManager.m_units.m_buffer[citizenUnitIndex].Empty()) 
                    {
                        return citizenUnitIndex;
                    }
                }
                citizenUnitIndex = nextCitizenUnitIndex;
            }

            return 0;
        }

        private int GetAverageResidentRequirement(ref Building data, ImmaterialResourceManager.Resource resource) 
        {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            uint citizenUnit = data.m_citizenUnits;
            uint numCitizenUnits = citizenManager.m_units.m_size;
            int counter = 0;
            int requirement1 = 0;
            int requirement2 = 0;
            while ((int) citizenUnit != 0) 
            {
                uint num5 = citizenManager.m_units.m_buffer[citizenUnit].m_nextUnit;
                if ((citizenManager.m_units.m_buffer[citizenUnit].m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None) 
                {
                    int residentRequirement1 = 0;
                    int residentRequirement2 = 0;
                    for (int index = 0; index < 5; ++index) 
                    {
                        uint citizen = citizenManager.m_units.m_buffer[citizenUnit].GetCitizen(index);
                        if ((int) citizen != 0 && !citizenManager.m_citizens.m_buffer[citizen].Dead) 
                        {
                            residentRequirement1 += GetResidentRequirement(resource, ref citizenManager.m_citizens.m_buffer[citizen]);
                            ++residentRequirement2;
                        }
                    }
                    if (residentRequirement2 == 0) 
                    {
                        requirement1 += 100;
                        ++requirement2;
                    } 
                    else 
                    {
                        requirement1 += residentRequirement1;
                        requirement2 += residentRequirement2;
                    }
                }
                citizenUnit = num5;
                if (++counter > numCitizenUnits) 
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
            if (requirement2 != 0)
                return (requirement1 + (requirement2 >> 1)) / requirement2;
            return 0;
        }

        private int GetResidentRequirement(ImmaterialResourceManager.Resource resource, ref Citizen citizen) 
        {
            switch (resource) 
            {
                case ImmaterialResourceManager.Resource.HealthCare:
                    return Citizen.GetHealthCareRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
                case ImmaterialResourceManager.Resource.FireDepartment:
                    return Citizen.GetFireDepartmentRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
                case ImmaterialResourceManager.Resource.PoliceDepartment:
                    return Citizen.GetPoliceDepartmentRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
                case ImmaterialResourceManager.Resource.EducationElementary:
                    Citizen.AgePhase agePhase1 = Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age);
                    if (agePhase1 < Citizen.AgePhase.Teen0)
                        return Citizen.GetEducationRequirement(agePhase1);
                    return 0;
                case ImmaterialResourceManager.Resource.EducationHighSchool:
                    Citizen.AgePhase agePhase2 = Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age);
                    if (agePhase2 >= Citizen.AgePhase.Teen0 && agePhase2 < Citizen.AgePhase.Young0)
                        return Citizen.GetEducationRequirement(agePhase2);
                    return 0;
                case ImmaterialResourceManager.Resource.EducationUniversity:
                    Citizen.AgePhase agePhase3 = Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age);
                    if (agePhase3 >= Citizen.AgePhase.Young0)
                        return Citizen.GetEducationRequirement(agePhase3);
                    return 0;
                case ImmaterialResourceManager.Resource.DeathCare:
                    return Citizen.GetDeathCareRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
                case ImmaterialResourceManager.Resource.PublicTransport:
                    return Citizen.GetTransportRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
                case ImmaterialResourceManager.Resource.Entertainment:
                    return Citizen.GetEntertainmentRequirement(Citizen.GetAgePhase(citizen.EducationLevel, citizen.Age));
                default:
                    return 100;
            }
        }

        public override float GetEventImpact(ushort buildingID, ref Building data, ImmaterialResourceManager.Resource resource, float amount) 
        {
            if ((data.m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown)) != Building.Flags.None)
                return 0.0f;
            switch (resource) 
            {
                case ImmaterialResourceManager.Resource.HealthCare:
                    int residentRequirement1 = GetAverageResidentRequirement(ref data, resource);
                    int local1;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local1);
                    int num1 = ImmaterialResourceManager.CalculateResourceEffect(local1, residentRequirement1, 500, 20, 40);
                    return Mathf.Clamp((ImmaterialResourceManager.CalculateResourceEffect(local1 + Mathf.RoundToInt(amount), residentRequirement1, 500, 20, 40) - num1) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.FireDepartment:
                    int residentRequirement2 = GetAverageResidentRequirement(ref data, resource);
                    int local2;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local2);
                    int num2 = ImmaterialResourceManager.CalculateResourceEffect(local2, residentRequirement2, 500, 20, 40);
                    return Mathf.Clamp((ImmaterialResourceManager.CalculateResourceEffect(local2 + Mathf.RoundToInt(amount), residentRequirement2, 500, 20, 40) - num2) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.PoliceDepartment:
                    int residentRequirement3 = GetAverageResidentRequirement(ref data, resource);
                    int local3;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local3);
                    int num3 = ImmaterialResourceManager.CalculateResourceEffect(local3, residentRequirement3, 500, 20, 40);
                    return Mathf.Clamp((ImmaterialResourceManager.CalculateResourceEffect(local3 + Mathf.RoundToInt(amount), residentRequirement3, 500, 20, 40) - num3) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.EducationElementary:
                case ImmaterialResourceManager.Resource.EducationHighSchool:
                case ImmaterialResourceManager.Resource.EducationUniversity:
                    int residentRequirement4 = GetAverageResidentRequirement(ref data, resource);
                    int local4;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local4);
                    int num4 = ImmaterialResourceManager.CalculateResourceEffect(local4, residentRequirement4, 500, 20, 40);
                    return Mathf.Clamp((ImmaterialResourceManager.CalculateResourceEffect(local4 + Mathf.RoundToInt(amount), residentRequirement4, 500, 20, 40) - num4) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.DeathCare:
                    int residentRequirement5 = GetAverageResidentRequirement(ref data, resource);
                    int local5;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local5);
                    int num5 = ImmaterialResourceManager.CalculateResourceEffect(local5, residentRequirement5, 500, 10, 20);
                    return Mathf.Clamp((ImmaterialResourceManager.CalculateResourceEffect(local5 + Mathf.RoundToInt(amount), residentRequirement5, 500, 10, 20) - num5) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.PublicTransport:
                    int residentRequirement6 = GetAverageResidentRequirement(ref data, resource);
                    int local6;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local6);
                    int num6 = ImmaterialResourceManager.CalculateResourceEffect(local6, residentRequirement6, 500, 20, 40);
                    return Mathf.Clamp((ImmaterialResourceManager.CalculateResourceEffect(local6 + Mathf.RoundToInt(amount), residentRequirement6, 500, 20, 40) - num6) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.NoisePollution:
                    int local7;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local7);
                    int num7 = local7 * 100 / (int) byte.MaxValue;
                    return Mathf.Clamp((Mathf.Clamp(local7 + Mathf.RoundToInt(amount), 0, (int) byte.MaxValue) * 100 / (int) byte.MaxValue - num7) / 50f, -1f, 1f);
                case ImmaterialResourceManager.Resource.Entertainment:
                    int residentRequirement7 = GetAverageResidentRequirement(ref data, resource);
                    int local8;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local8);
                    int num8 = ImmaterialResourceManager.CalculateResourceEffect(local8, residentRequirement7, 500, 30, 60);
                    return Mathf.Clamp((ImmaterialResourceManager.CalculateResourceEffect(local8 + Mathf.RoundToInt(amount), residentRequirement7, 500, 30, 60) - num8) / 30f, -1f, 1f);
                case ImmaterialResourceManager.Resource.Abandonment:
                    int local9;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local9);
                    int num9 = ImmaterialResourceManager.CalculateResourceEffect(local9, 15, 50, 10, 20);
                    return Mathf.Clamp((ImmaterialResourceManager.CalculateResourceEffect(local9 + Mathf.RoundToInt(amount), 15, 50, 10, 20) - num9) / 50f, -1f, 1f);
                default:
                    return 0f;
            }
        }

        public override float GetEventImpact(ushort buildingID, ref Building data, NaturalResourceManager.Resource resource, float amount) 
        {
            if ((data.m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown)) != Building.Flags.None)
                return 0.0f;
            if (resource != NaturalResourceManager.Resource.Pollution)
                return 0f;
            Singleton<NaturalResourceManager>.instance.CheckPollution(data.m_position, out byte groundPollution);
            int num = groundPollution * 100 / byte.MaxValue;
            return Mathf.Clamp((Mathf.Clamp(groundPollution + Mathf.RoundToInt(amount), 0, byte.MaxValue) * 100 / byte.MaxValue - num) / 50f, -1f, 1f);
        }

        public void GetConsumptionRates(Randomizer randomizer, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation) 
        {
            electricityConsumption = 16;
            waterConsumption = 35;
            sewageAccumulation = 35;
            garbageAccumulation = 20;
            incomeAccumulation = 0;
            if (electricityConsumption != 0)
                electricityConsumption = Mathf.Max(100, productionRate * electricityConsumption + randomizer.Int32(70U)) / 100;
            if (waterConsumption != 0) 
            {
                int waterAndSewageConsumptionModifier = randomizer.Int32(70U);
                waterConsumption = Mathf.Max(100, productionRate * waterConsumption + waterAndSewageConsumptionModifier) / 100;
                if (sewageAccumulation != 0)
                    sewageAccumulation = Mathf.Max(100, productionRate * sewageAccumulation + waterAndSewageConsumptionModifier) / 100;
            } 
            else if (sewageAccumulation != 0)
                sewageAccumulation = Mathf.Max(100, productionRate * sewageAccumulation + randomizer.Int32(70U)) / 100;
            if (garbageAccumulation != 0)
                garbageAccumulation = Mathf.Max(100, productionRate * garbageAccumulation + randomizer.Int32(70U)) / 100;
            if (incomeAccumulation == 0)
                return;
            incomeAccumulation = productionRate * incomeAccumulation;
        }

        public void GetOccupancyDetails(ref Building data, out int numResidents, out int numApartmentsOccupied) 
        {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            uint citizenUnitIndex = data.m_citizenUnits;
            uint numCitizenUnits = citizenManager.m_units.m_size;
            numResidents = 0;
            numApartmentsOccupied = 0;
            int counter = 0;

            // Calculate number of occupied apartments and total number of residents
            while ((int) citizenUnitIndex != 0) 
            {
                uint nextCitizenUnitIndex = citizenManager.m_units.m_buffer[citizenUnitIndex].m_nextUnit;
                if ((citizenManager.m_units.m_buffer[citizenUnitIndex].m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None) 
                {
                    bool occupied = false;
                    for (int index = 0; index < 5; ++index) 
                    {
                        uint citizenId = citizenManager.m_units.m_buffer[citizenUnitIndex].GetCitizen(index);
                        if (citizenId != 0) 
                        {
                            occupied = true;
                            numResidents++;
                        }
                    }
                    if (occupied) 
                    {
                        numApartmentsOccupied++;
                    }
                }
                citizenUnitIndex = nextCitizenUnitIndex;
                if (++counter > numCitizenUnits) 
                {
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                    break;
                }
            }
        }

        public int GetModifiedCapacity(ushort buildingID)
        {
            var buildingRecord = HousingManager.GetBuildingRecord(buildingID);
            return buildingRecord.NumOfApartments;
        }

        public void ValidateCapacity(ushort buildingId, ref Building data, bool shouldCreateApartments) 
        {
            int numApartmentsExpected = GetModifiedCapacity(buildingId);
            
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            uint citizenUnitIndex = data.m_citizenUnits;
            uint lastCitizenUnitIndex = 0;
            int numApartmentsFound = 0;

            // Count the number of apartments
            while ((int) citizenUnitIndex != 0) 
            {
                uint nextCitizenUnitIndex = citizenManager.m_units.m_buffer[citizenUnitIndex].m_nextUnit;
                if ((citizenManager.m_units.m_buffer[citizenUnitIndex].m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None) 
                {
                    numApartmentsFound++;
                }
                lastCitizenUnitIndex = citizenUnitIndex;
                citizenUnitIndex = nextCitizenUnitIndex;
            }

            Logger.LogInfo(Logger.LOG_BARRACKS_CAPACITY, "BarracksAI.ValidateCapacity -- Checking Expected Capacity {0} vs Current Capacity {1} for Building {2}", numApartmentsExpected, numApartmentsFound, buildingId);
            // Check to see if the correct amount of apartments are present, otherwise adjust accordingly
            if (numApartmentsFound == numApartmentsExpected) 
            {
                return;
            } 
            else if (numApartmentsFound < numApartmentsExpected) 
            {
                if (shouldCreateApartments) 
                {
                    // Only create apartments after a building is already loaded, otherwise let EnsureCitizenUnits to create them
                    CreateApartments(numApartmentsExpected - numApartmentsFound, buildingId, lastCitizenUnitIndex);
                }
            } 
            else 
            {
                DeleteApartments(numApartmentsFound - numApartmentsExpected, ref data);
            }
        }

        private void CreateApartments(int numApartmentsToCreate, ushort buildingId, uint lastCitizenUnitIndex) 
        {
            Logger.LogInfo(Logger.LOG_BARRACKS_CAPACITY, "BarracksAI.CreateApartments -- Creating {0} Apartments", numApartmentsToCreate);
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            citizenManager.CreateUnits(out uint firstUnit, ref Singleton<SimulationManager>.instance.m_randomizer, buildingId, 0, numApartmentsToCreate);
            citizenManager.m_units.m_buffer[lastCitizenUnitIndex].m_nextUnit = firstUnit;
        }

        private void DeleteApartments(int numApartmentsToDelete, ref Building data) 
        {
            Logger.LogInfo(Logger.LOG_BARRACKS_CAPACITY, "BarracksAI.DeleteApartments -- Deleting {0} Apartments", numApartmentsToDelete);
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            
            // Always start with the second to avoid loss of pointer from the building to the first unit
            uint prevUnit = data.m_citizenUnits;
            uint citizenUnitIndex = citizenManager.m_units.m_buffer[data.m_citizenUnits].m_nextUnit;

            // First try to delete empty apartments
            while (numApartmentsToDelete > 0 && (int) citizenUnitIndex != 0) 
            {
                bool deleted = false;
                uint nextCitizenUnitIndex = citizenManager.m_units.m_buffer[citizenUnitIndex].m_nextUnit;
                if ((citizenManager.m_units.m_buffer[citizenUnitIndex].m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None) 
                {
                    if (citizenManager.m_units.m_buffer[citizenUnitIndex].Empty()) 
                    {
                        DeleteApartment(citizenUnitIndex, ref citizenManager.m_units.m_buffer[citizenUnitIndex], prevUnit);
                        numApartmentsToDelete--;
                        deleted = true;
                    }
                }
                if(!deleted) 
                {
                    prevUnit = citizenUnitIndex;
                }
                citizenUnitIndex = nextCitizenUnitIndex;
            }

            // Check to see if enough apartments were deleted
            if(numApartmentsToDelete == 0) 
            {
                return;
            }

            Logger.LogInfo(Logger.LOG_BARRACKS_CAPACITY, "BarracksAI.DeleteApartments -- Deleting {0} Occupied Apartments", numApartmentsToDelete);
            // Still need to delete more apartments so start deleting apartments with people in them...
            // Always start with the second to avoid loss of pointer from the building to the first unit
            prevUnit = data.m_citizenUnits;
            citizenUnitIndex = citizenManager.m_units.m_buffer[data.m_citizenUnits].m_nextUnit;

            // Delete any apartments still available until the correct number is acheived
            while (numApartmentsToDelete > 0 && (int) citizenUnitIndex != 0) 
            {
                bool deleted = false;
                uint nextCitizenUnitIndex = citizenManager.m_units.m_buffer[citizenUnitIndex].m_nextUnit;
                if ((citizenManager.m_units.m_buffer[citizenUnitIndex].m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None) 
                {
                    DeleteApartment(citizenUnitIndex, ref citizenManager.m_units.m_buffer[citizenUnitIndex], prevUnit);
                    numApartmentsToDelete--;
                    deleted = true;
                }
                if (!deleted) 
                {
                    prevUnit = citizenUnitIndex;
                }
                citizenUnitIndex = nextCitizenUnitIndex;
            }
        }

        private void DeleteApartment(uint unit, ref CitizenUnit data, uint prevUnit) 
        {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;

            // Update the pointer to bypass this unit
            citizenManager.m_units.m_buffer[prevUnit].m_nextUnit = data.m_nextUnit;

            // Release all the citizens
            ReleaseUnitCitizen(data.m_citizen0, ref data);
            ReleaseUnitCitizen(data.m_citizen1, ref data);
            ReleaseUnitCitizen(data.m_citizen2, ref data);
            ReleaseUnitCitizen(data.m_citizen3, ref data);
            ReleaseUnitCitizen(data.m_citizen4, ref data);

            // Release the Unit
            data = new CitizenUnit();
            citizenManager.m_units.ReleaseItem(unit);
        }

        private void ReleaseUnitCitizen(uint citizen, ref CitizenUnit data) 
        {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;

            if ((int) citizen == 0) 
            {
                return;
            }
            if ((data.m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None) 
            {
                citizenManager.m_citizens.m_buffer[citizen].m_homeBuilding = 0;
            }
            if ((data.m_flags & (CitizenUnit.Flags.Work | CitizenUnit.Flags.Student)) != CitizenUnit.Flags.None) 
            {
                citizenManager.m_citizens.m_buffer[citizen].m_workBuilding = 0;
            }
            if ((data.m_flags & CitizenUnit.Flags.Visit) != CitizenUnit.Flags.None) 
            {
                citizenManager.m_citizens.m_buffer[citizen].m_visitBuilding = 0;
            }
            if ((data.m_flags & CitizenUnit.Flags.Vehicle) == CitizenUnit.Flags.None) 
            {
                return;
            }
            citizenManager.m_citizens.m_buffer[citizen].m_vehicle = 0;
        }

    }
}