using System;
using System.Configuration;
using System.Threading.Tasks;

namespace SparkSharp
{
    class SimpleExample
    {
        internal static async Task ExampleAsync()
        {
            var settings = ConfigurationManager.AppSettings;
            var clusterName = settings["ClusterName"];
            var password = settings["ClusterPassword"];

            using (var client = new HdInsightClient(clusterName, "admin", password))
            using (var session = await client.CreateSessionAsync(SessionConfiguration.GetDefaultCosmosConfiguration()))
            {
                var sum = await session.ExecuteStatementAsync<int>("val res = 1 + 1\nprintln(res)");

                Console.WriteLine(sum);
            }

            Console.ReadKey();
        }
    }
}