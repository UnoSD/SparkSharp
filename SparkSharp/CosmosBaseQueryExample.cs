using System;
using System.Collections.Generic;
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
                const string sparkSql = "SELECT contactIdentifier AS Item1, COUNT(*) AS Item2 FROM cosmos GROUP BY contactIdentifier ORDER BY COUNT(*) DESC LIMIT 20";
                const string cosmosSqlQuery = "SELECT cosmos.contactIdentifier FROM cosmos";

                // Create session and warm up
                await cosmos.QuerySparkSqlAsync<(int, decimal)>(sparkSql, cosmosSqlQuery);

                var query = (Func<Task<IEnumerable<(int, decimal)>>>)(() => cosmos.QuerySparkSqlAsync<(int, decimal)>(sparkSql, cosmosSqlQuery));

                var result = await query.RunAndTimeExecution();

                Console.WriteLine($"Elpsed: {result.Elapsed}");

                result.Result.ToList().ForEach(t => Console.WriteLine($"{t.Item1}:{t.Item2}"));
            }

            Console.ReadKey();
        }
    }
}