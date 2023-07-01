using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gpt.Labs.Helpers.Extensions
{
    public static class ExceptionExtensions
    {
        #region Public Methods and Operators

        public static void LogEvent(this string name, Dictionary<string, string> properties)
        {
#if DEBUG
            Debug.WriteLine("Information: " + name);
            Console.WriteLine("Information: " + name);

            foreach (var property in properties)
            {
                Debug.WriteLine(property.Key + ": " + property.Value);
                Console.WriteLine(property.Key + ": " + property.Value);
            }
#endif

            Analytics.TrackEvent(name, properties);
        }

        public static void LogError(this Exception ex)
        {
#if DEBUG
            Debug.WriteLine(ex.ToString());    
            Console.WriteLine(ex);
#endif

            Crashes.TrackError(ex);
        }

        public static void LogError(this Exception ex, string message)
        {
#if DEBUG
            Debug.WriteLine(ex.ToString());
            Console.WriteLine(ex);            
#endif

            var properties = new Dictionary<string, string> { ["ErrorCustomTitle"] = message };
            Crashes.TrackError(ex, properties);
        }

        public static void LogError(this Exception ex, string message, Dictionary<string, string> properties)
        {
#if DEBUG
            Debug.WriteLine(ex.ToString());      
            Console.WriteLine(ex);
#endif

            properties.Add("ErrorCustomTitle", message);
            Crashes.TrackError(ex, properties);
        }

        public static void LogWarning(this Exception ex)
        {
#if DEBUG
            Debug.WriteLine("Warn: " + ex);      
            Console.WriteLine("Warn: " + ex);
#endif
        }

        public static void LogWarning(this string message)
        {
#if DEBUG
            Debug.WriteLine("Warn: " + message);
            Console.WriteLine("Warn: " + message);
#endif
        }


        public static void LogInfo(this string message)
        {
#if DEBUG
            Debug.WriteLine("Info: " + message);
            Console.WriteLine("Info: " + message);
#endif
        }

        #endregion
    }
}
