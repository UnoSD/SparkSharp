using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SparkSharp
{
    class CosmosExample
    {
        internal static async Task ExampleAsync()
        {
            var settings = ConfigurationManager.AppSettings;
            var clusterName = settings["ClusterName"];
            var password = settings["ClusterPassword"];
            var cosmosSettings = new CosmosCollectionSettings
            {
                Name = settings["CosmosName"],
                Key = settings["CosmosKey"],
                Database = settings["CosmosDatabase"],
                Collection = settings["CosmosCollection"],
                PreferredRegions = settings["CosmosPreferredRegions"]
            };

            using (var client = new HdInsightClient(clusterName, "admin", password))
            using (var cosmos = new CosmosDbLivySession(client, cosmosSettings, SessionConfiguration.GetDefaultCosmosConfiguration()))
            {
                const string sql = "SELECT contactIdentifier AS ContactIdentifier, COUNT(*) AS Count FROM cosmos GROUP BY contactIdentifier ORDER BY COUNT(*) DESC LIMIT 20";

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var results = await cosmos.QuerySparkSqlAsync<IEnumerable<Result>>(sql);

                Console.WriteLine($"Elpsed: {stopwatch.Elapsed}");

                results.ToList().ForEach(t => Console.WriteLine($"{t.ContactIdentifier}:{t.Count}"));
            }

            Console.ReadKey();
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        class Result
        {
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public int ContactIdentifier { get; set; }
            public decimal Count { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local
        }
    }
}