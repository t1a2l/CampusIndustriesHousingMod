using ColossalFramework;
using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace CampusIndustriesHousingMod.Utils
{
    public class HousingConfig
    {
        public List<Housing> HousingSettings = new List<Housing>();

        private const string optionsFileName = "CampusIndustriesHousingMod.xml";

        public bool ShowPanel { get; set; } = true;

        static XmlSerializer ser_ => new XmlSerializer(typeof(HousingConfig));

        static HousingConfig config_;
        static public HousingConfig Config => config_ ??= Deserialize() ?? new HousingConfig(); 
        
        public static void Reset() => config_ = new HousingConfig();


        public void AddGlobalSettings(Housing housing)
        {
            if (!HousingSettings.Contains(housing))
            {
                HousingSettings.Add(housing);
            }
            else
            {
               var index = HousingSettings.FindIndex(x => x.Name == housing.Name && x.Type == housing.Type);
               HousingSettings[index] = housing;
            }
        }

        public void RemoveGlobalSettings(Housing housing)
        {
            if (HousingSettings.Contains(housing))
            {
                HousingSettings.Remove(housing);
            }
        }

        public static HousingConfig Deserialize() 
        {
            try 
            {
                if (File.Exists(GetXMLPath()))
			    {
				    using (FileStream stream = new FileStream(GetXMLPath(), FileMode.Open, FileAccess.Read))
				    {
					    return ser_.Deserialize(stream) as HousingConfig;
				    }
			    }
            } 
            catch (Exception ex) 
            { 
                 Logger.logError("CampusIndustriesHousingMod: " + ex.Message);
            }
            return null;
        }
        
        public void Serialize() 
        {
            try 
            {
                using FileStream stream = new FileStream(GetXMLPath(), FileMode.Create, FileAccess.Write);
			    ser_.Serialize(stream, this);   
            } 
            catch(Exception ex) 
            { 
                Logger.logError("CampusIndustriesHousingMod: " + ex.Message);
            }
        }

        public static string GetXMLPath()
        {
            string file_path = "";

		    string fileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		    string CO_path = Path.Combine(fileName, "Collosal Order");
            string CS_path = Path.Combine(CO_path, "Cities_Skylines");
            string Addons_path = Path.Combine(CS_path, "Addons");
		    string mods_path = Path.Combine(Addons_path, "Mods");

            if (!Directory.Exists(mods_path))
            {
                IEnumerable<PluginManager.PluginInfo> plugins = PluginManager.instance.GetPluginsInfo();
                string assemblyPath = plugins.First().modPath;

                if (!assemblyPath.IsNullOrWhiteSpace())
			    {
				    string fullPath = Path.GetFullPath(Path.Combine(assemblyPath, "..\\"));
				    mods_path = Path.Combine(fullPath, "2854004833");
			    }
            }

            if (Directory.Exists(mods_path))
            {
                string folder_path = Path.Combine(mods_path, "CampusIndustriesHousingMod");
                file_path = Path.Combine(folder_path, optionsFileName);
            }

		    return file_path;
        }


    }

    [XmlRoot("Housing")]
    public class Housing
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Type")]
        public string Type { get; set; }

        [XmlAttribute("numOfApartments")]
        public int NumOfApartments { get; set; }

        [XmlAttribute("WorkPlaceCount0")]
        public int WorkPlaceCount0 { get; set; }

        [XmlAttribute("WorkPlaceCount1")]
        public int WorkPlaceCount1 { get; set; }

        [XmlAttribute("WorkPlaceCount2")]
        public int WorkPlaceCount2 { get; set; }

        [XmlAttribute("WorkPlaceCount3")]
        public int WorkPlaceCount3 { get; set; }
    }

}
