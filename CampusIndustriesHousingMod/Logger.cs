using System;
using UnityEngine;

namespace CampusIndustriesHousingMod 
{
    internal static class Logger 
    {
        private static readonly string Prefix = "CampusIndustriesHousingMod: ";

        public static readonly bool LOG_BASE = true;

        public static readonly bool LOG_SERIALIZATION = false;
        public static readonly bool LOG_CHANCES = false;
        public static readonly bool LOG_OPTIONS = false;

        public static readonly bool LOG_BARRACKS_CAPACITY = false;
        public static readonly bool LOG_BARRACKS_INCOME = false;
        public static readonly bool LOG_BARRACKS_PRODUCTION = false;
        public static readonly bool LOG_BARRACKS_SIMULATION = false;

        public static readonly bool LOG_DORMS_CAPACITY = false;
        public static readonly bool LOG_DORMS_INCOME = false;
        public static readonly bool LOG_DORMS_PRODUCTION = false;
        public static readonly bool LOG_DORMS_SIMULATION = false;

        public static readonly bool LOG_CAMPUS = false;
        public static readonly bool LOG_INDUSTRY = false;

        public static readonly bool LOG_STUDENTS_MANAGER = false;
        public static readonly bool LOG_WORKERS_MANAGER = false;



        public static void LogInfo(bool shouldLog, string message, params object[] args) 
        {
            if (shouldLog) 
            {
                LogInfo(message, args);
            }
        }

        internal static void LogInfo(object LOG_OPTIONS, string v) 
        {
            throw new NotImplementedException();
        }

        public static void LogInfo(string message, params object[] args) 
        {
            Debug.Log(Prefix + string.Format(message, args));
        }

        public static void LogWarning(bool shouldLog, string message, params object[] args) 
        {
            if (shouldLog) 
            {
                LogWarning(message, args);
            }
        }

        public static void LogWarning(string message, params object[] args) 
        {
            Debug.LogWarning(Prefix + string.Format(message, args));
        }

        public static void LogError(bool shouldLog, string message, params object[] args) 
        {
            if (shouldLog) 
            {
                LogError(message, args);
            }
        }

        public static void LogError(string message, params object[] args) 
        {
            Debug.LogError(Prefix + string.Format(message, args));
        }
    }
}