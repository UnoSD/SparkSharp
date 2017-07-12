using System.Collections.Specialized;

namespace SparkSharp
{
    class CosmosSettings
    {
        public static CosmosCollectionSettings GetSettings(NameValueCollection settings) =>
            new CosmosCollectionSettings
            {
                Name = settings["CosmosName"],
                Key = settings["CosmosKey"],
                Database = settings["CosmosDatabase"],
                Collection = settings["CosmosCollection"],
                PreferredRegions = settings["CosmosPreferredRegions"]
            };
    }
}