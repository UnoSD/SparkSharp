using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace SparkSharp
{
    static class CosmosPoolingExample
    {
        internal static async Task ExampleAsync(NameValueCollection settings)
        {
            using (var client = new HdInsightClient(settings["ClusterName"], settings["ClusterUsername"], settings["ClusterPassword"]))
            using(var cosmos = new CosmosDbLivyObjectPooledSession(client, CosmosSettings.GetSettings(settings), CosmosExampleSessionConfiguration.GetConfiguration(), 2))
            {
                const string sql = "SELECT * FROM cosmos LIMIT 1";

                var tasks = new[]
                            {
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