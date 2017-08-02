using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace SparkSharp
{
    static class SimpleExample
    {
        internal static async Task ExampleAsync(NameValueCollection settings)
        {
            using (var client = new LivyClient("http://127.0.0.1:8998", settings["ClusterUsername"], settings["ClusterPassword"]))
            using (var session = await client.CreateSessionAsync(SimpleExampleSessionConfiguration.GetConfiguration()))
            {
                var sum = await session.ExecuteStatementAsync<int>("val res = 1 + 1\nprintln(res)");

                Console.WriteLine(sum);
            }

            Console.ReadKey();
        }
    }
}