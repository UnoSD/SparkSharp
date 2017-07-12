// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace SparkSharp
{
    class CosmosExample
    {
        internal static async Task ExampleAsync()
        {
            var settings = ConfigurationManager.AppSettings;
            var cosmosSettings = CosmosSettings.GetSettings(settings);

            using (var client = new HdInsightClient(settings["ClusterName"], settings["ClusterUsername"], settings["ClusterPassword"]))
            using (var cosmos = new CosmosDbLivySession(client, cosmosSettings, CosmosExampleSessionConfiguration.GetConfiguration()))
            {
                const string sql = "SELECT contactIdentifier AS ContactIdentifier, COUNT(*) AS Count FROM cosmos GROUP BY contactIdentifier ORDER BY COUNT(*) DESC LIMIT 20";
                
                var result = await cosmos.QuerySparkSqlWithMetricsAsync<Result>(sql);

                Console.WriteLine($"Elpsed: {result.Elapsed}");

                result.Result.ToList().ForEach(t => Console.WriteLine($"{t.ContactIdentifier}:{t.Count}"));
            }

            Console.ReadKey();
        }
        
        class Result
        {
            public int ContactIdentifier { get; set; }
            public decimal Count { get; set; }
        }
    }
}