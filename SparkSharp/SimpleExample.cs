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

            using (var client = new HdInsightClient(settings["ClusterName"], settings["ClusterUsername"], settings["ClusterPassword"]))
            using (var session = await client.CreateSessionAsync(SimpleExampleSessionConfiguration.GetConfiguration()))
            {
                var sum = await session.ExecuteStatementAsync<int>("val res = 1 + 1\nprintln(res)");

                Console.WriteLine(sum);
            }

            Console.ReadKey();
        }
    }
}