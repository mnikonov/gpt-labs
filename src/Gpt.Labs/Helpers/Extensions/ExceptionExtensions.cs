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

#if USE_APPCENTER
            Analytics.TrackEvent(name, properties);
#endif
        }

        public static void LogError(this Exception ex)
        {
#if DEBUG
            Debug.WriteLine(ex.ToString());    
            Console.WriteLine(ex);
#endif

#if USE_APPCENTER
            Crashes.TrackError(ex);
#endif
        }

        public static void LogError(this Exception ex, string message)
        {
#if DEBUG
            Debug.WriteLine(ex.ToString());
            Console.WriteLine(ex);            
#endif

#if USE_APPCENTER
            var properties = new Dictionary<string, string> { ["ErrorCustomTitle"] = message };
            Crashes.TrackError(ex, properties);
#endif
        }

        public static void LogError(this Exception ex, string message, Dictionary<string, string> properties)
        {
#if DEBUG
            Debug.WriteLine(ex.ToString());      
            Console.WriteLine(ex);
#endif

#if USE_APPCENTER
            properties.Add("ErrorCustomTitle", message);
            Crashes.TrackError(ex, properties);
#endif
        }

        public static void LogWarning(this Exception ex)
        {
#if DEBUG
            Debug.WriteLine("Warning: " + ex);      
            Console.WriteLine("Warning: " + ex);
#endif

#if USE_APPCENTER
            AppCenterLog.Warn("Warning", ex.Message, ex);
#endif
        }

        public static void LogWarning(this string message)
        {
#if DEBUG
            Debug.WriteLine("Warning: " + message);
            Console.WriteLine("Warning: " + message);
#endif
            
#if USE_APPCENTER
            AppCenterLog.Warn("Warning", message);
#endif
        }


        public static void LogInfo(this string message)
        {
#if DEBUG
            Debug.WriteLine("Information: " + message);
            Console.WriteLine("Information: " + message);
#endif

#if USE_APPCENTER
            AppCenterLog.Info(message, message);
#endif
        }

        #endregion

        #region Private Methods

        private static void SetData(this Exception ex, Dictionary<string, string> properties)
        {
            if (properties != null)
            {
                if (properties.ContainsKey("ErrorCustomTitle"))
                {
                    ex.Data["ErrorCustomTitle"] = properties["ErrorCustomTitle"];
                }

                foreach (var item in properties)
                {
                    if (item.Key != "ErrorCustomTitle" && !string.IsNullOrEmpty(item.Key) && item.Value != null)
                    {
                        ex.Data[item.Key] = item.Value;
                    }
                }
            }
        }

        private static string DictToString(this Dictionary<string, string> items)
        {
            var formatted = "\r\n\r\nParameters: \r\n\r\n";
            foreach (var item in items)
            {
                formatted += item.Key + " = '" + item.Value + "' \r\n";
            }

            return formatted;
        }

        #endregion
    }
}
