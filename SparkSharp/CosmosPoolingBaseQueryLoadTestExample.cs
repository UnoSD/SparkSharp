using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SparkSharp
{
    static class CosmosPoolingBaseQueryLoadTestExample
    {
        internal static async Task ExampleAsync(NameValueCollection settings)
        {
            const int pooledSessions = 15;
            ServicePointManager.DefaultConnectionLimit = pooledSessions * 3;

            using (var client = new HdInsightClient(settings["ClusterName"], settings["ClusterUsername"], settings["ClusterPassword"]))
            using (var cosmos = new CosmosDbLivyObjectPooledSession(client, CosmosSettings.GetSettings(settings), CosmosExampleSessionConfiguration.GetConfiguration(), pooledSessions))
            {
                await CloseAllSessions(client);

                await InitializeAllPoolSessionsAsync(cosmos, pooledSessions);

                const string baseQuery = "SELECT c.contactIdentifier, c.json.products FROM c";
                const string sql = "SELECT * FROM cosmos LIMIT 1";

                var query = (Func<Task<IEnumerable<dynamic>>>)(() => cosmos.QuerySparkSqlAsync<dynamic>(sql, baseQuery));
                
                var tasks = new List<(int index, Task<(TimeSpan Elapsed, IEnumerable<dynamic> Result)> task)>();

                for (var i = 0; i < 20; i++)
                {
                    tasks.Add((i, Task.Run(async () =>
                    {
                        try
                        {
                            return await query.RunAndTimeExecution();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            return (TimeSpan.Zero, Enumerable.Empty<dynamic>());
                        }
                    })));

                    await Task.Delay(200); // 300/min => 5/sec => 1/200ms
                }

                await tasks.Select(t => t.task);

                tasks.Select(t => (t.index, t.task.Result.Elapsed))
                     .OrderBy(t => t.Item1)
                     .Select(t => $"[{t.Item2}] - {t.Item1}")
                     .ToList()
                     .ForEach(Console.WriteLine);

                Console.WriteLine("Press any key to dispose the sessions...");
                Console.ReadKey();
            }

            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }

        static async Task CloseAllSessions(ILivyClient client) => 
            await (await client.GetSessionsAsync()).Select(session => session.CloseAsync());

        static async Task InitializeAllPoolSessionsAsync(CosmosDbLivyObjectPooledSession cosmos, int pooledSessions)
        {
            var sessions = await Enumerable.Repeat(cosmos, pooledSessions)
                                           .Select(async pool =>
                                           {
                                               try { return await pool.RentAsync(); }
                                               catch { return null; }
                                           });

            foreach (var session in sessions.Where(session => session != null))
                cosmos.Return(session);
        }
    }
}