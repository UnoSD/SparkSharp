using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace SparkSharp
{
    static class CosmosPoolingBaseQueryExample
    {
        internal static async Task ExampleAsync(NameValueCollection settings)
        {
            const int pooledSessions = 5;
            ServicePointManager.DefaultConnectionLimit = pooledSessions * 3;

            using (var client = new HdInsightClient(settings["ClusterName"], settings["ClusterUsername"], settings["ClusterPassword"]))
            using (var cosmos = new CosmosDbLivyObjectPooledSession(client, CosmosSettings.GetSettings(settings), CosmosExampleSessionConfiguration.GetConfiguration(), pooledSessions))
            {
                const string baseQuery = "SELECT c.contactIdentifier, c.json FROM c";
                const string sql = @"
SELECT contactIdentifier, SUM(price)
FROM cosmos
LATERAL VIEW explode(json.products.price) tab AS price
GROUP BY contactIdentifier
HAVING SUM(price) > 5000.00
";

                var tasks = new[]
                            {
                                cosmos.QuerySparkSqlAsync<Result>(sql, baseQuery),
                                cosmos.QuerySparkSqlAsync<Result>(sql, baseQuery),
                                cosmos.QuerySparkSqlAsync<Result>(sql, baseQuery),
                                cosmos.QuerySparkSqlAsync<Result>(sql, baseQuery),
                                cosmos.QuerySparkSqlAsync<Result>(sql, baseQuery),
                                cosmos.QuerySparkSqlAsync<Result>(sql, baseQuery),
                                cosmos.QuerySparkSqlAsync<Result>(sql, baseQuery),
                                cosmos.QuerySparkSqlAsync<Result>(sql, baseQuery),
                                cosmos.QuerySparkSqlAsync<Result>(sql, baseQuery),
                                cosmos.QuerySparkSqlAsync<Result>(sql, baseQuery)
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