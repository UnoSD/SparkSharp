namespace SparkSharp
{
    public class CosmosExampleSessionConfiguration
    {
        public static LivySessionConfiguration GetConfiguration() => 
            new LivySessionConfiguration
            {
                Kind = LivySessionKind.Spark,
                Name = "SparkSharp testing",
                ExecutorMemory = null,
                ExecutorCores = null,
                NumExecutors = null,
                DriverCores = null,
                Jars =
                {
                    "wasb:///example/jars/azure-cosmosdb-spark-0.0.3-SNAPSHOT.jar",
                    "wasb:///example/jars/azure-documentdb-1.12.0.jar"
                },
                Conf =
                {
                    ["spark.jars.packages"] = "graphframes:graphframes:0.4.0-spark2.0-s_2.11",
                    ["spark.jars.excludes"] = "org.scala-lang:scala-reflect",
                    ["spark.dynamicAllocation.enabled"] = "true",
                    ["spark.shuffle.service.enabled"] = "true"
                }
            };
    }
}