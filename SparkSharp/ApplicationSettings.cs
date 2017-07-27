using System;
using System.Configuration;

namespace SparkSharp
{
    static class ApplicationSettings
    {
        public static void SetupSettings()
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var mySettings = configuration.AppSettings.Settings;
            var firstRun = mySettings["FirstRun"]?.Value.AsBooleanOrDefault(true) ?? true;

            if (!(firstRun || Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.S))
                return;

            void SetConfig(string key, string value)
            {
                if (mySettings[key] == null)
                    mySettings.Add(key, value);
                else
                    mySettings[key].Value = value;
            }

            void SetConfigFromUserInput(string key, string defaultValue = null)
            {
                defaultValue = mySettings[key]?.Value ?? defaultValue;
                
                Console.Write($"Insert {key} [{defaultValue}]: ");
                var value = Console.ReadLine();
                SetConfig(key, string.IsNullOrWhiteSpace(value) ? defaultValue : value);
            }

            SetConfigFromUserInput("ClusterName");
            SetConfigFromUserInput("ClusterUsername", "admin");
            SetConfigFromUserInput("ClusterPassword");
            SetConfigFromUserInput("CosmosName");
            SetConfigFromUserInput("CosmosKey");
            SetConfigFromUserInput("CosmosDatabase");
            SetConfigFromUserInput("CosmosCollection");
            SetConfigFromUserInput("CosmosPreferredRegions", "North Europe");
            SetConfigFromUserInput("AutoStartExample", "SimpleExample");

            SetConfig("FirstRun", "False");

            configuration.Save();
        }
    }
}