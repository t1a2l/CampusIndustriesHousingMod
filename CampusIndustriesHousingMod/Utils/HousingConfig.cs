using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace CampusIndustriesHousingMod.Utils
{
    public class HousingConfig
    {
        public List<Housing> HousingSettings = [];

        private const string optionsFileName = "CampusIndustriesHousingMod.xml";

        public bool ShowPanel { get; set; } = false;

        static XmlSerializer Ser_ => new(typeof(HousingConfig));

        static HousingConfig config_;
        static public HousingConfig Config => config_ ??= Deserialize() ?? new HousingConfig(); 
        
        public static void Reset() => config_ = new HousingConfig();


        public Housing GetGlobalSettings(BuildingInfo buildingInfo)
        {
            string BuildingAIstr = buildingInfo.GetAI().GetType().Name;
            var index = HousingSettings.FindIndex(x => x.Name == buildingInfo.name && x.BuildingAI == BuildingAIstr);
            if (index != -1)
            {
                return HousingSettings[index];
            }
            return default;
        }

        public void SetGlobalSettings(Housing housing)
        {
            var index = HousingSettings.FindIndex(x => x.Name == housing.Name && x.BuildingAI == housing.BuildingAI);
            if (index != -1)
            {
                HousingSettings[index] = housing;
            }
        }

        public void CreateGlobalSettings(Housing housing)
        {
            var index = HousingSettings.FindIndex(x => x.Name == housing.Name && x.BuildingAI == housing.BuildingAI);
            if (index == -1)
            {
                HousingSettings.Add(housing);
            }
        }

        public void ClearGlobalSettings()
        {
            HousingSettings.Clear();
        }

        public static HousingConfig Deserialize() 
        {
            try 
            {
                if (File.Exists(GetXMLPath()))
			    {
                    using FileStream stream = new(GetXMLPath(), FileMode.Open, FileAccess.Read);
                    return Ser_.Deserialize(stream) as HousingConfig;
                }
            } 
            catch (Exception ex) 
            { 
                 Logger.LogError("CampusIndustriesHousingMod: " + ex.Message);
            }
            return null;
        }
        
        public void Serialize() 
        {
            try 
            {
                using FileStream stream = new FileStream(GetXMLPath(), FileMode.Create, FileAccess.Write);
			    Ser_.Serialize(stream, this);   
            } 
            catch(Exception ex) 
            { 
                Logger.LogError("CampusIndustriesHousingMod: " + ex.Message);
            }
        }

        public static string GetXMLPath()
        {
		    string fileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		    string CO_path = Path.Combine(fileName, "Colossal Order");
            string CS_path = Path.Combine(CO_path, "Cities_Skylines");
            string file_path = Path.Combine(CS_path, optionsFileName);
            return file_path;
        }

    }

    [XmlRoot("Housing")]
    public class Housing
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("BuildingAI")]
        public string BuildingAI { get; set; }

        [XmlAttribute("numOfApartments")]
        public int NumOfApartments { get; set; }
    }

}
