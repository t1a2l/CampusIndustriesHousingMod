using System;
using System.Text;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using CampusIndustriesHousingMod.Utils;

namespace CampusIndustriesHousingMod.AI
{
    public class DormsAI : CampusBuildingAI {

        private Randomizer randomizer = new(97);

        [CustomizableProperty("Number of Apartments")]
        public int numApartments = 60;
        private float capacityModifier = 1.0f;

        public override Color GetColor(ushort buildingId, ref Building data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode) 
        {
            // This is a copy from ResidentialBuildingAI
            InfoManager.InfoMode infoModeCopy = infoMode;
            switch (infoModeCopy) 
            {
                case InfoManager.InfoMode.Health:
                    if (this.ShowConsumption(buildingId, ref data) && (int) data.m_citizenCount != 0)
                        return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor, (float) Citizen.GetHealthLevel((int) data.m_health) * 0.2f);
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
                default:
                    switch (infoModeCopy - 17) 
                    {
                        case InfoManager.InfoMode.None:
                            if (this.ShowConsumption(buildingId, ref data)) {
                                return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_neutralColor, Color.Lerp(Singleton<ZoneManager>.instance.m_properties.m_zoneColors[2], Singleton<ZoneManager>.instance.m_properties.m_zoneColors[3], 0.5f) * 0.5f, (float) (0.200000002980232 + (double) Math.Max(0, 2) * 0.200000002980232));
                            }
                            return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                        case InfoManager.InfoMode.Water:
                            if (!this.ShowConsumption(buildingId, ref data) || (int) data.m_citizenCount == 0)
                                return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                            InfoManager.SubInfoMode currentSubMode = Singleton<InfoManager>.instance.CurrentSubMode;
                            int num4;
                            int num5;
                            if (currentSubMode == InfoManager.SubInfoMode.Default) 
                            {
                                num4 = (int) data.m_education1 * 100;
                                num5 = (int) data.m_teens + (int) data.m_youngs + (int) data.m_adults + (int) data.m_seniors;
                            } 
                            else if (currentSubMode == InfoManager.SubInfoMode.WaterPower) 
                            {
                                num4 = (int) data.m_education2 * 100;
                                num5 = (int) data.m_youngs + (int) data.m_adults + (int) data.m_seniors;
                            } 
                            else 
                            {
                                num4 = (int) data.m_education3 * 100;
                                num5 = (int) data.m_youngs * 2 / 3 + (int) data.m_adults + (int) data.m_seniors;
                            }
                            if (num5 != 0)
                                num4 = (num4 + (num5 >> 1)) / num5;
                            int num6 = Mathf.Clamp(num4, 0, 100);
                            return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor, (float) num6 * 0.01f);
                        default:
                            return this.handleOtherColors(buildingId, ref data, infoMode, subInfoMode);
                    }
            }
        }

        private Color handleOtherColors(ushort buildingId, ref Building data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode) 
        {
            switch (infoMode) 
            {
                case InfoManager.InfoMode.Happiness:
                    if (ShowConsumption(buildingId, ref data)) 
                    {
                        return Color.Lerp(Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_negativeColor, Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int) infoMode].m_targetColor, (float) Citizen.GetHappinessLevel((int) data.m_happiness) * 0.25f);
                    }
                    return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                case InfoManager.InfoMode.Garbage:
                    if (m_garbageAccumulation == 0)
                        return Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                    return base.GetColor(buildingId, ref data, infoMode, subInfoMode);
                default:
                    return base.GetColor(buildingId, ref data, infoMode, subInfoMode);
            }
        }

        public override void CreateBuilding(ushort buildingID, ref Building data)
	    {
		    base.CreateBuilding(buildingID, ref data);
		    int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
		    Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, getModifiedCapacity(buildingID, ref data), workCount, 0, 0, StudentCount * 5 / 4);
        }

        public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
	    {
		    base.BuildingLoaded(buildingID, ref data, version);
           
            // Validate the capacity and adjust accordingly - but don't create new units, that will be done by EnsureCitizenUnits
            float capcityModifier = Mod.getInstance().getOptionsManager().getDormsCapacityModifier();
            this.updateCapacity(capcityModifier);
            this.validateCapacity(buildingID, ref data, false);

		    int workCount =  m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
		    EnsureCitizenUnits(buildingID, ref data, getModifiedCapacity(buildingID, ref data), workCount, 0, 0);
	    }

        public override void EndRelocating(ushort buildingID, ref Building data)
	    {
		    base.EndRelocating(buildingID, ref data);

            // Validate the capacity and adjust accordingly - but don't create new units, that will be done by EnsureCitizenUnits
            float capacityModifier = Mod.getInstance().getOptionsManager().getDormsCapacityModifier();
            this.updateCapacity(capacityModifier);
            this.validateCapacity(buildingID, ref data, false);

		    int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
		    EnsureCitizenUnits(buildingID, ref data, getModifiedCapacity(buildingID, ref data), workCount, 0, 0);
	    }

        public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData) 
        {
            base.SimulationStep(buildingID, ref buildingData, ref frameData);
        }

        protected  override void SimulationStepActive(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
	    {
			Citizen.BehaviourData behaviour = default(Citizen.BehaviourData);
			int aliveCount = 0;
			int totalCount = 0;
            int homeCount = 0;
            int aliveWorkerCount = 0;
	        int totalWorkerCount = 0;
            int aliveHomeCount = 0;
            int emptyHomeCount = 0;

            GetHomeBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveCount, ref totalCount, ref homeCount, ref aliveHomeCount, ref emptyHomeCount);
            GetWorkBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount);

            DistrictManager districtManager = Singleton<DistrictManager>.instance;
            byte district = districtManager.GetDistrict(buildingData.m_position);
            DistrictPolicies.Services policies = districtManager.m_districts.m_buffer[(int) district].m_servicePolicies;

            DistrictPolicies.Taxation taxationPolicies = districtManager.m_districts.m_buffer[(int) district].m_taxationPolicies;
            DistrictPolicies.CityPlanning cityPlanning = districtManager.m_districts.m_buffer[(int) district].m_cityPlanningPolicies;
            DistrictPolicies.Special special = districtManager.m_districts.m_buffer[(int) district].m_specialPolicies;

            districtManager.m_districts.m_buffer[(int) district].m_servicePoliciesEffect |= policies & (DistrictPolicies.Services.PowerSaving | DistrictPolicies.Services.WaterSaving | DistrictPolicies.Services.SmokeDetectors | DistrictPolicies.Services.PetBan | DistrictPolicies.Services.Recycling | DistrictPolicies.Services.SmokingBan | DistrictPolicies.Services.ExtraInsulation | DistrictPolicies.Services.NoElectricity | DistrictPolicies.Services.OnlyElectricity);

            this.GetConsumptionRates(new Randomizer((int) buildingID), 100, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation);

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
                    heatingConsumption = Mathf.Max(1, modifiedElectricityConsumption + 2 >> 2);
            }

            // Handle Recylcing and Pets
            if (garbageAccumulation != 0) 
            {
                if ((policies & DistrictPolicies.Services.Recycling) != DistrictPolicies.Services.None) {
                    garbageAccumulation = (policies & DistrictPolicies.Services.PetBan) == DistrictPolicies.Services.None ? Mathf.Max(1, garbageAccumulation * 85 / 100) : Mathf.Max(1, garbageAccumulation * 7650 / 10000);
                    modifiedIncomeAccumulation = modifiedIncomeAccumulation * 95 / 100;
                } else if ((policies & DistrictPolicies.Services.PetBan) != DistrictPolicies.Services.None) {
                    garbageAccumulation = Mathf.Max(1, garbageAccumulation * 90 / 100);
                }
            }

            if ((int) buildingData.m_fireIntensity == 0) 
            {
                int maxMail = 100;
                int mailAccumulation = 1;
                int commonConsumptionValue = this.HandleCommonConsumption(buildingID, ref buildingData, ref frameData, ref modifiedElectricityConsumption, ref heatingConsumption, ref waterConsumption, ref modifiedSewageAccumulation, ref garbageAccumulation, ref mailAccumulation, maxMail, policies);
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
            float radius = (float) (buildingData.Width + buildingData.Length) * 2.5f;
            if (behaviour.m_healthAccumulation != 0) 
            {
                if (aliveCount != 0) 
                {
                    health = (behaviour.m_healthAccumulation + (aliveCount >> 1)) / aliveCount;
                }
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Health, behaviour.m_healthAccumulation, buildingData.m_position, radius);
            }
            Logger.LogInfo(Logger.LOG_SIMULATION, "DormsAI.SimulationStepActive -- health: {0}", health);

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
            Logger.LogInfo(Logger.LOG_SIMULATION, "DormsAI.SimulationStepActive -- wellbeing: {0}", wellbeing);

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
            Logger.LogInfo(Logger.LOG_SIMULATION, "DormsAI.SimulationStepActive -- happiness: {0}", happiness);

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

            HandleSick(buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalCount);
            HandleDead(buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalCount);

            // Handle Crime and Fire Factors
            int crimeAccumulation = behaviour.m_crimeAccumulation / (3 * getModifiedCapacity(buildingID, ref buildingData));
            if ((policies & DistrictPolicies.Services.RecreationalUse) != DistrictPolicies.Services.None) 
            {
                crimeAccumulation = crimeAccumulation * 3 + 3 >> 2;
            }
            this.HandleCrime(buildingID, ref buildingData, crimeAccumulation, aliveCount);
            int crimeBuffer = (int) buildingData.m_crimeBuffer;
            int crimeRate;
            if (aliveCount != 0) 
            {
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.Density, aliveCount, buildingData.m_position, radius);
                // num1
                int fireFactor = (behaviour.m_educated0Count * 30 + behaviour.m_educated1Count * 15 + behaviour.m_educated2Count * 10) / aliveCount + 50;
                if ((int) buildingData.m_crimeBuffer > aliveCount * 40) 
                {
                    fireFactor += 30;
                } 
                else if ((int) buildingData.m_crimeBuffer > aliveCount * 15) 
                {
                    fireFactor += 15;
                } 
                else if ((int) buildingData.m_crimeBuffer > aliveCount * 5) 
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

            districtManager.m_districts.m_buffer[(int) district].AddResidentialData(ref behaviour, aliveCount, health, happiness, crimeRate, homeCount, aliveHomeCount, emptyHomeCount, (int) this.m_info.m_class.m_level, modifiedElectricityConsumption, heatingConsumption, waterConsumption, modifiedSewageAccumulation, garbageAccumulation, modifiedIncomeAccumulation, Mathf.Min(100, (int) buildingData.m_garbageBuffer / 50), (int) buildingData.m_waterPollution * 100 / (int) byte.MaxValue, this.m_info.m_class.m_subService);

            // Handle custom maintenance in addition to the standard maintenance handled in the base class
            handleAdditionalMaintenanceCost(buildingID, ref buildingData);
		    
            base.SimulationStepActive(buildingID, ref buildingData, ref frameData);
            HandleFire(buildingID, ref buildingData, ref frameData, policies);
	    }
        
        protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount) 
        {
            base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);    
            if (finalProductionRate == 0)
		    {
			    return;
		    }
            getOccupancyDetails(ref buildingData, out int numResidents, out int numApartmentsOccupied);

            // Make sure there are no problems
            if ((buildingData.m_problems & (Notification.Problem1.MajorProblem | Notification.Problem1.Electricity | Notification.Problem1.ElectricityNotConnected | Notification.Problem1.Fire | Notification.Problem1.Water | Notification.Problem1.WaterNotConnected | Notification.Problem1.TurnedOff)) != Notification.Problem1.None) 
            {
                return;
            }

            StudentManager studentManager = StudentManager.getInstance();
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;

            // Fetch a family with students
            uint[] familyWithStudents = studentManager.getFamilyWithStudents(buildingData);
            if (familyWithStudents != null) 
            {
                Logger.LogInfo(Logger.LOG_PRODUCTION, "------------------------------------------------------------");
                Logger.LogInfo(Logger.LOG_PRODUCTION, "DormsAI.ProduceGoods -- Student: {0}", string.Join(", ", Array.ConvertAll(familyWithStudents, item => item.ToString())));
                // Check move in chance
                bool shouldMoveIn = MoveInProbabilityHelper.checkIfShouldMoveIn(familyWithStudents, ref buildingData, ref randomizer, "student");

                if(shouldMoveIn)
                {
                    // get all the students in the family
                    List<uint> studentsList = new();
                    foreach (uint familyMember in familyWithStudents) 
                    {
                        if (studentManager.isCampusAreaStudent(familyMember))
                        {
                            Logger.LogInfo(Logger.LOG_PRODUCTION, "DormsAI.ProduceGoods -- familyMember: {0} is a student", familyMember);
                            studentsList.Add(familyMember);
                        }
                    }

                    // add a student to an apartment randomly
                    for(int i = 0; i < studentsList.Count; i++)
                    {
                        uint dormApartmentId = getCitizenUnit(ref buildingData);
                        if (dormApartmentId == 0) 
                        {
                            return;
                        }
                        uint studentId = studentsList[i];
                        Logger.LogInfo(Logger.LOG_PRODUCTION, "DormsAI.ProduceGoods -- Moving In: {0}", studentId);
                        citizenManager.m_citizens.m_buffer[studentId].SetHome(studentId, buildingID, dormApartmentId);
                        studentManager.doneProcessingStudent(studentId);
                    }
                }

            }

            // Fetch Students who needs to move out of the apartment dorm
            uint[] DormApartmentStudents = studentManager.getDormApartmentStudents(buildingData);
            if (DormApartmentStudents != null) 
            {
                Logger.LogInfo(Logger.LOG_PRODUCTION, "------------------------------------------------------------");
                Logger.LogInfo(Logger.LOG_PRODUCTION, "DormsAI.ProduceGoods -- DormApartmentStudents: {0}", string.Join(", ", Array.ConvertAll(DormApartmentStudents, item => item.ToString())));

                foreach (uint studentId in DormApartmentStudents)
                {
                    Logger.LogInfo(Logger.LOG_PRODUCTION, "DormsAI.ProduceGoods -- Moving Out: {0}", studentId);
                    if(studentId != 0)
                    {
                        citizenManager.m_citizens.m_buffer[studentId].SetHome(studentId, 0, 0);
                        studentManager.doneProcessingStudent(studentId);
                    } 
                }
            }

        }
        
        public override string GetLocalizedStats(ushort buildingID, ref Building data) 
        {
            getOccupancyDetails(ref data, out int numResidents, out int numApartmentsOccupied);
            // Get Worker Data
            Citizen.BehaviourData workerBehaviourData = new Citizen.BehaviourData();
            int aliveWorkerCount = 0;
            int totalWorkerCount = 0;
            GetWorkBehaviour(buildingID, ref data, ref workerBehaviourData, ref aliveWorkerCount, ref totalWorkerCount);
		    GetStudentCount(buildingID, ref data, out var count, out var capacity, out var global);
            StringBuilder stringBuilder = new();
		    if (capacity != 0)
		    {
			    stringBuilder.Append(LocaleFormatter.FormatGeneric("AIINFO_STUDENTS", count, capacity));
                stringBuilder.Append(Environment.NewLine);
			    string localeID = "AIINFO_UNIVERSITY_STUDENTCOUNT";
			    stringBuilder.Append(LocaleFormatter.FormatGeneric(localeID, global));
                stringBuilder.Append(Environment.NewLine);
		    }
		    if (m_bonusEffect == BonusEffects.Academics)
		    {
			    DistrictManager instance2 = Singleton<DistrictManager>.instance;
			    byte park = instance2.GetPark(data.m_position);
			    if (park != 0 && instance2.m_parks.m_buffer[park].m_parkType == m_campusType)
			    {
				    int academicWorkBaseChance = Singleton<DistrictManager>.instance.m_properties.m_parkProperties.m_campusProperties.m_academicWorkBaseChance;
				    instance2.m_parks.m_buffer[park].CalculateAcademicWorkChance(out var academicStaffProjectedChance, out var buildingsChance);
				    string text2 = Mathf.RoundToInt((float)academicWorkBaseChance + academicStaffProjectedChance + buildingsChance) + "%";
                    stringBuilder.Append(LocaleFormatter.FormatGeneric("AIINFO_ACADEMICWORKSCHANCE", text2));
                    stringBuilder.Append(Environment.NewLine);
			    }
		    }
            stringBuilder.Append(string.Format("Apartments Occupied: {0} of {1}", numApartmentsOccupied, getModifiedCapacity(buildingID, ref data)));
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(string.Format("Number of Residents: {0}", numResidents));
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(Environment.NewLine);
            if(m_workPlaceCount0 > 0)
            {
                stringBuilder.Append(string.Format("Uneducated Workers: {0} of {1}", workerBehaviourData.m_educated0Count, m_workPlaceCount0));
                stringBuilder.Append(Environment.NewLine);
            }
            if(m_workPlaceCount1 > 0)
            {
                stringBuilder.Append(string.Format("Educated Workers: {0} of {1}", workerBehaviourData.m_educated1Count, m_workPlaceCount1));
                stringBuilder.Append(Environment.NewLine);
            }
            if(m_workPlaceCount2 > 0)
            {
                stringBuilder.Append(string.Format("Well Educated Workers: {0} of {1}", workerBehaviourData.m_educated2Count, m_workPlaceCount2));
                stringBuilder.Append(Environment.NewLine);
            }
            if(m_workPlaceCount3 > 0)
            {
                stringBuilder.Append(string.Format("Highly Educated Workers: {0} of {1}", workerBehaviourData.m_educated3Count, m_workPlaceCount3));
            }
            return stringBuilder.ToString();
        }

        private int getCustomMaintenanceCost(ushort buildingID, ref Building buildingData) 
        {
            int originalAmount = -(this.m_maintenanceCost * 100);

            Mod mod = Mod.getInstance();
            if (mod == null) 
            {
                return 0;
            }

            OptionsManager optionsManager = mod.getOptionsManager();
            if (optionsManager == null) 
            {
                return 0;
            }

            getOccupancyDetails(ref buildingData, out int numResidents, out int numApartmentsOccupied);
            float capacityModifier = (float) numApartmentsOccupied / (float) getModifiedCapacity(buildingID,ref buildingData);
            int modifiedAmount = (int) ((float) originalAmount * capacityModifier);

            int amount = 0;
            switch (optionsManager.getDormsIncomeModifier()) 
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
            
            Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetMaintenanceCost(ref amount, this.m_info.m_class.m_service, this.m_info.m_class.m_subService, this.m_info.m_class.m_level);
            Logger.LogInfo(Logger.LOG_INCOME, "getCustomMaintenanceCost - building: {0} - calculated maintenance amount: {1}", buildingData.m_buildIndex, amount);

            return amount;
        }

        public void handleAdditionalMaintenanceCost(ushort buildingID, ref Building buildingData) 
        {
            int amount = getCustomMaintenanceCost(buildingID, ref buildingData);
            if (amount == 0) 
            {
                return;
            }

            int productionRate = (int) buildingData.m_productionRate;
            int budget = Singleton<EconomyManager>.instance.GetBudget(this.m_info.m_class);
            amount = amount / 100;
            amount = productionRate * budget / 100 * amount / 100;
            Logger.LogInfo(Logger.LOG_INCOME, "getCustomMaintenanceCost - building: {0} - adjusted maintenance amount: {1}", buildingData.m_buildIndex, amount);

            if ((buildingData.m_flags & Building.Flags.Original) == Building.Flags.None && amount != 0) 
            {
                int result = Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Maintenance, amount, this.m_info.m_class);
            }
        }

        private uint getCitizenUnit(ref Building data) {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            uint citizenUnitIndex = data.m_citizenUnits;
            while ((int) citizenUnitIndex != 0) 
            {
                uint nextCitizenUnitIndex = citizenManager.m_units.m_buffer[citizenUnitIndex].m_nextUnit;
                if ((citizenManager.m_units.m_buffer[citizenUnitIndex].m_flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None) 
                {
                    for (int i = 0; i < 5; i++)
                    {
                        uint citizenId = citizenManager.m_units.m_buffer[citizenUnitIndex].GetCitizen(i);
                        if(citizenId == 0)
                        {
                            return citizenUnitIndex;
                        }
                    }
                }
                citizenUnitIndex = nextCitizenUnitIndex;
            }

            return 0;
        }

        private int GetAverageResidentRequirement(ushort buildingID, ref Building data, ImmaterialResourceManager.Resource resource) 
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
                    CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
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
                    int residentRequirement1 = GetAverageResidentRequirement(buildingID, ref data, resource);
                    int local1;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local1);
                    int num1 = ImmaterialResourceManager.CalculateResourceEffect(local1, residentRequirement1, 500, 20, 40);
                    return Mathf.Clamp((float) (ImmaterialResourceManager.CalculateResourceEffect(local1 + Mathf.RoundToInt(amount), residentRequirement1, 500, 20, 40) - num1) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.FireDepartment:
                    int residentRequirement2 = GetAverageResidentRequirement(buildingID, ref data, resource);
                    int local2;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local2);
                    int num2 = ImmaterialResourceManager.CalculateResourceEffect(local2, residentRequirement2, 500, 20, 40);
                    return Mathf.Clamp((float) (ImmaterialResourceManager.CalculateResourceEffect(local2 + Mathf.RoundToInt(amount), residentRequirement2, 500, 20, 40) - num2) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.PoliceDepartment:
                    int residentRequirement3 = GetAverageResidentRequirement(buildingID, ref data, resource);
                    int local3;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local3);
                    int num3 = ImmaterialResourceManager.CalculateResourceEffect(local3, residentRequirement3, 500, 20, 40);
                    return Mathf.Clamp((float) (ImmaterialResourceManager.CalculateResourceEffect(local3 + Mathf.RoundToInt(amount), residentRequirement3, 500, 20, 40) - num3) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.EducationElementary:
                case ImmaterialResourceManager.Resource.EducationHighSchool:
                case ImmaterialResourceManager.Resource.EducationUniversity:
                    int residentRequirement4 = GetAverageResidentRequirement(buildingID, ref data, resource);
                    int local4;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local4);
                    int num4 = ImmaterialResourceManager.CalculateResourceEffect(local4, residentRequirement4, 500, 20, 40);
                    return Mathf.Clamp((float) (ImmaterialResourceManager.CalculateResourceEffect(local4 + Mathf.RoundToInt(amount), residentRequirement4, 500, 20, 40) - num4) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.DeathCare:
                    int residentRequirement5 = GetAverageResidentRequirement(buildingID, ref data, resource);
                    int local5;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local5);
                    int num5 = ImmaterialResourceManager.CalculateResourceEffect(local5, residentRequirement5, 500, 10, 20);
                    return Mathf.Clamp((float) (ImmaterialResourceManager.CalculateResourceEffect(local5 + Mathf.RoundToInt(amount), residentRequirement5, 500, 10, 20) - num5) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.PublicTransport:
                    int residentRequirement6 = GetAverageResidentRequirement(buildingID, ref data, resource);
                    int local6;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local6);
                    int num6 = ImmaterialResourceManager.CalculateResourceEffect(local6, residentRequirement6, 500, 20, 40);
                    return Mathf.Clamp((float) (ImmaterialResourceManager.CalculateResourceEffect(local6 + Mathf.RoundToInt(amount), residentRequirement6, 500, 20, 40) - num6) / 20f, -1f, 1f);
                case ImmaterialResourceManager.Resource.NoisePollution:
                    int local7;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local7);
                    int num7 = local7 * 100 / (int) byte.MaxValue;
                    return Mathf.Clamp((float) (Mathf.Clamp(local7 + Mathf.RoundToInt(amount), 0, (int) byte.MaxValue) * 100 / (int) byte.MaxValue - num7) / 50f, -1f, 1f);
                case ImmaterialResourceManager.Resource.Entertainment:
                    int residentRequirement7 = GetAverageResidentRequirement(buildingID, ref data, resource);
                    int local8;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local8);
                    int num8 = ImmaterialResourceManager.CalculateResourceEffect(local8, residentRequirement7, 500, 30, 60);
                    return Mathf.Clamp((float) (ImmaterialResourceManager.CalculateResourceEffect(local8 + Mathf.RoundToInt(amount), residentRequirement7, 500, 30, 60) - num8) / 30f, -1f, 1f);
                case ImmaterialResourceManager.Resource.Abandonment:
                    int local9;
                    Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(resource, data.m_position, out local9);
                    int num9 = ImmaterialResourceManager.CalculateResourceEffect(local9, 15, 50, 10, 20);
                    return Mathf.Clamp((float) (ImmaterialResourceManager.CalculateResourceEffect(local9 + Mathf.RoundToInt(amount), 15, 50, 10, 20) - num9) / 50f, -1f, 1f);
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
            byte groundPollution;
            Singleton<NaturalResourceManager>.instance.CheckPollution(data.m_position, out groundPollution);
            int num = (int) groundPollution * 100 / (int) byte.MaxValue;
            return Mathf.Clamp((float) (Mathf.Clamp((int) groundPollution + Mathf.RoundToInt(amount), 0, (int) byte.MaxValue) * 100 / (int) byte.MaxValue - num) / 50f, -1f, 1f);
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

        public void getOccupancyDetails(ref Building data, out int numResidents, out int numApartmentsOccupied) 
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

        public void updateCapacity(float newCapacityModifier) 
        {
            Logger.LogInfo(Logger.LOG_OPTIONS, "DormsAI.updateCapacity -- Updating capacity with modifier: {0}", newCapacityModifier);
            // Set the capcityModifier and check to see if the value actually changes
            if (Interlocked.Exchange(ref capacityModifier, newCapacityModifier) == newCapacityModifier) 
            {
                // Capcity has already been set to this value, nothing to do
                Logger.LogInfo(Logger.LOG_OPTIONS, "DormsAI.updateCapacity -- Skipping capacity change because the value was already set");
                return;
            }
        }

        public int getModifiedCapacity(ushort buildingID, ref Building data) 
        {
            var dorms = data.Info.GetAI() as DormsAI;
            return capacityModifier > 0 ? (int) (dorms.numApartments * capacityModifier) : dorms.numApartments;
        }

        public void validateCapacity(ushort buildingId, ref Building data, bool shouldCreateApartments) 
        {
            int numApartmentsExpected = getModifiedCapacity(buildingId, ref data);
            
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

            Logger.LogInfo(Logger.LOG_CAPACITY_MANAGEMENT, "DormsAI.validateCapacity -- Checking Expected Capacity {0} vs Current Capacity {1} for Building {2}", numApartmentsExpected, numApartmentsFound, buildingId);
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
                    createApartments((numApartmentsExpected - numApartmentsFound), buildingId, ref data, lastCitizenUnitIndex);
                }
            } 
            else 
            {
                deleteApartments((numApartmentsFound - numApartmentsExpected), buildingId, ref data);
            }
        }

        private void createApartments(int numApartmentsToCreate, ushort buildingId, ref Building data, uint lastCitizenUnitIndex) 
        {
            Logger.LogInfo(Logger.LOG_CAPACITY_MANAGEMENT, "DormsAI.createApartments -- Creating {0} Apartments", numApartmentsToCreate);
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            citizenManager.CreateUnits(out uint firstUnit, ref Singleton<SimulationManager>.instance.m_randomizer, buildingId, (ushort) 0, numApartmentsToCreate, 0, 0, 0, 0);
            citizenManager.m_units.m_buffer[lastCitizenUnitIndex].m_nextUnit = firstUnit;
        }

        private void deleteApartments(int numApartmentsToDelete, ushort buildingId, ref Building data) 
        {
            Logger.LogInfo(Logger.LOG_CAPACITY_MANAGEMENT, "DormsAI.deleteApartments -- Deleting {0} Apartments", numApartmentsToDelete);
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
                        deleteApartment(citizenUnitIndex, ref citizenManager.m_units.m_buffer[citizenUnitIndex], prevUnit);
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

            Logger.LogInfo(Logger.LOG_CAPACITY_MANAGEMENT, "BarracksAI.deleteApartments -- Deleting {0} Occupied Apartments", numApartmentsToDelete);
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
                    deleteApartment(citizenUnitIndex, ref citizenManager.m_units.m_buffer[citizenUnitIndex], prevUnit);
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

        private void deleteApartment(uint unit, ref CitizenUnit data, uint prevUnit) 
        {
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;

            // Update the pointer to bypass this unit
            citizenManager.m_units.m_buffer[prevUnit].m_nextUnit = data.m_nextUnit;

            // Release all the citizens
            releaseUnitCitizen(data.m_citizen0, ref data);
            releaseUnitCitizen(data.m_citizen1, ref data);
            releaseUnitCitizen(data.m_citizen2, ref data);
            releaseUnitCitizen(data.m_citizen3, ref data);
            releaseUnitCitizen(data.m_citizen4, ref data);

            // Release the Unit
            data = new CitizenUnit();
            citizenManager.m_units.ReleaseItem(unit);
        }

        private void releaseUnitCitizen(uint citizen, ref CitizenUnit data) 
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