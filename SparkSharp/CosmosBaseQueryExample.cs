using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace SparkSharp
{
    static class CosmosBaseQueryExample
    {
        internal static async Task ExampleAsync(NameValueCollection settings)
        {
            using (var client = new HdInsightClient(settings["ClusterName"], settings["ClusterUsername"], settings["ClusterPassword"]))
            using (var cosmos = new CosmosDbLivySession(client, CosmosSettings.GetSettings(settings), CosmosExampleSessionConfiguration.GetConfiguration()))
            {
                const string sparkSql = "SELECT contactIdentifier AS ContactIdentifier, COUNT(*) AS Count FROM cosmos GROUP BY contactIdentifier ORDER BY COUNT(*) DESC LIMIT 20";
                const string cosmosSqlQuery = "SELECT c.contactIdentifier FROM c";

                // Create session and warm up
                await cosmos.QuerySparkSqlAsync<Result>(sparkSql, cosmosSqlQuery);

                var result = await cosmos.QuerySparkSqlWithMetricsAsync<Result>(sparkSql, cosmosSqlQuery);

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