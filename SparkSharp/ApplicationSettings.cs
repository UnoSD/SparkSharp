using System.Configuration;

namespace SparkSharp
{
    static class ApplicationSettings
    {
        public static void SetupSettings()
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var mySettings = configuration.AppSettings.Settings;

            void SetConfig(string key, string value)
            {
                if (mySettings[key] == null)
                    mySettings.Add(key, value);
                else
                    mySettings[key].Value = value;
            }

            SetConfig("ClusterName", "");
            SetConfig("ClusterUsername", "admin");
            SetConfig("ClusterPassword", "");
            SetConfig("CosmosName", "");
            SetConfig("CosmosKey", "");
            SetConfig("CosmosDatabase", "");
            SetConfig("CosmosCollection", "");
            SetConfig("CosmosPreferredRegions", "North Europe");
            configuration.Save();
        }
    }
}