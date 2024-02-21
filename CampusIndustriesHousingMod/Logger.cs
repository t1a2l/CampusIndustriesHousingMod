using System;
using UnityEngine;

namespace CampusIndustriesHousingMod 
{
    internal static class Logger 
    {
        private static readonly string Prefix = "CampusIndustriesHousingMod: ";

        public static readonly bool LOG_BASE = true;

        public static readonly bool LOG_INITIALIZER = true;
        public static readonly bool LOG_SERIALIZATION = true;
        public static readonly bool LOG_CHANCES = true;
        public static readonly bool LOG_OPTIONS = true;
        public static readonly bool LOG_DATA = true;
        public static readonly bool LOG_CAPACITY_MANAGEMENT = true;
        public static readonly bool LOG_INCOME = true;

        public static readonly bool LOG_PRODUCTION = true;
        public static readonly bool LOG_SIMULATION = true;
        public static readonly bool LOG_CAMPUS = true;
        public static readonly bool LOG_INDUSTRY = true;

        public static readonly bool LOG_STUDENTS = true;
        public static readonly bool LOG_WORKERS = true;



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