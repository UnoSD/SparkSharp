using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace SparkSharp
{
    static class CosmosPoolingExample
    {
        internal static async Task ExampleAsync(NameValueCollection settings)
        {
            const int pooledSessions = 3;
            ServicePointManager.DefaultConnectionLimit = pooledSessions * 3;

            using (var client = new HdInsightClient(settings["ClusterName"], settings["ClusterUsername"], settings["ClusterPassword"]))
            using(var cosmos = new CosmosDbLivyObjectPooledSession(client, CosmosSettings.GetSettings(settings), CosmosExampleSessionConfiguration.GetConfiguration(), pooledSessions))
            {
                const string sql = "SELECT contactIdentifier AS ContactIdentifier, COUNT(*) AS Count FROM cosmos GROUP BY contactIdentifier ORDER BY COUNT(*) DESC LIMIT 20";

                var tasks = new[]
                            {
                                cosmos.QuerySparkSqlAsync<Result>(sql),
                                cosmos.QuerySparkSqlAsync<Result>(sql),
                                cosmos.QuerySparkSqlAsync<Result>(sql),
                                cosmos.QuerySparkSqlAsync<Result>(sql),
                                cosmos.QuerySparkSqlAsync<Result>(sql),
                                cosmos.QuerySparkSqlAsync<Result>(sql),
                                cosmos.QuerySparkSqlAsync<Result>(sql),
                                cosmos.QuerySparkSqlAsync<Result>(sql),
                                cosmos.QuerySparkSqlAsync<Result>(sql),
                                cosmos.QuerySparkSqlAsync<Result>(sql)
                            };

                var results = await Task.WhenAll(tasks);

                results.SelectMany(r => r).ToList().ForEach(t => Console.WriteLine($"{t.ContactIdentifier}:{t.Count}"));
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