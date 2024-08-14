using System;
using UnityEngine;

namespace CampusIndustriesHousingMod 
{
    internal static class Logger 
    {
        private static readonly string Prefix = "CampusIndustriesHousingMod: ";

        public static readonly bool LOG_BASE = true;

        public static readonly bool LOG_INITIALIZER = false;
        public static readonly bool LOG_SERIALIZATION = true;
        public static readonly bool LOG_CHANCES = false;
        public static readonly bool LOG_OPTIONS = false;
        public static readonly bool LOG_DATA = true;
        public static readonly bool LOG_CAPACITY_MANAGEMENT = false;
        public static readonly bool LOG_INCOME = false;

        public static readonly bool LOG_PRODUCTION = false;
        public static readonly bool LOG_SIMULATION = false;
        public static readonly bool LOG_CAMPUS = false;
        public static readonly bool LOG_INDUSTRY = false;

        public static readonly bool LOG_STUDENTS = false;
        public static readonly bool LOG_WORKERS = false;



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
                Logger.LogWarning(message, args);
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
                Logger.LogError(message, args);
            }
        }

        public static void LogError(string message, params object[] args) 
        {
            Debug.LogError(Prefix + string.Format(message, args));
        }
    }
}