using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace SparkSharp
{
    static class Program
    {
        static void Main() => MainAsync().GetAwaiter().GetResult();

        static async Task MainAsync()
        {
            var config = new LivySessionConfiguration
            {
                Kind = LivySessionKind.Spark,
                Name = "SparkSharp testing",
                ExecutorMemory = "8G",
                ExecutorCores = 2,
                NumExecutors = 2,
                DriverCores = 2,
                Jars =
                {
                    "wasb:///example/jars/azure-cosmosdb-spark-0.0.3-SNAPSHOT.jar",
                    "wasb:///example/jars/azure-documentdb-1.10.0.jar"
                },
                Conf =
                {
                    ["spark.jars.packages"] = "graphframes:graphframes:0.4.0-spark2.0-s_2.11",
                    ["spark.jars.excludes"] = "org.scala-lang:scala-reflect"
                }
            };

            var settings = ConfigurationManager.AppSettings;
            var clusterName = settings["ClusterName"];
            var password = settings["ClusterPassword"];

            var cosmosName = settings["CosmosName"];
            var cosmosKey = settings["CosmosKey"];
            var cosmosDatabase = settings["CosmosDatabase"];
            var cosmosCollection = settings["CosmosCollection"];
            var cosmosPreferredRegions = settings["CosmosPreferredRegions"];

            using (var client = new HdInsightClient(clusterName, "admin", password))
            using (var session = await client.CreateSessionAsync(config))
            {
                var sum = await session.ExecuteStatementAsync<int>("val res = 1 + 1\nprintln(res)");

                Console.WriteLine(sum);

                const string sql = "SELECT contactIdentifier AS ContactIdentifier, SUM(json.order_total) AS OrderTotal FROM cosmos GROUP BY contactIdentifier";

                var cosmos = await session.ExecuteCosmosDbSparkSqlQueryAsync<IEnumerable<Result>>
                (
                    cosmosName,
                    cosmosKey,
                    cosmosDatabase,
                    cosmosCollection,
                    cosmosPreferredRegions,
                    sql
                );

                cosmos.ToList().ForEach(t => Console.WriteLine($"{t.ContactIdentifier}:{t.OrderTotal}"));
            }

            Console.ReadKey();
        }

        static Task<T> ExecuteCosmosDbSparkSqlQueryAsync<T>(this ILivySession session, string cosmosName, string key, string database, string collection, string preferredRegions, string sql) =>
            session.ExecuteStatementAsync<T>($@"
import com.microsoft.azure.cosmosdb.spark.schema._
import com.microsoft.azure.cosmosdb.spark._
import com.microsoft.azure.cosmosdb.spark.config.Config

val config = Config(Map(""Endpoint""         -> ""https://{cosmosName}.documents.azure.com:443/"",
                        ""Masterkey""        -> ""{key}"",
                        ""Database""         -> ""{database}"",
                        ""preferredRegions"" -> ""{preferredRegions};"",
                        ""Collection""       -> ""{collection}"", 
                        ""SamplingRatio""    -> ""1.0""))

val coll = spark.sqlContext.read.cosmosDB(config)
coll.createOrReplaceTempView(""cosmos"")

val docs = spark.sql(""{sql}"")
docs.createOrReplaceTempView(""docs"")

println(docs.toJSON.collect.mkString(""["", "","", ""]""))
");

        // ReSharper disable once ClassNeverInstantiated.Local
        class Result
        {
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public int ContactIdentifier { get; set; }
            public decimal OrderTotal { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local
        }
    }
}
