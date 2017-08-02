namespace SparkSharp
{
    class CosmosExampleSessionConfiguration
    {
        internal static LivySessionConfiguration GetConfiguration() => 
            new LivySessionConfiguration
            {
                Kind = LivySessionKind.Spark,
                Name = "SparkSharp testing",
                ExecutorMemory = "11G",
                ExecutorCores = 15,
                NumExecutors = 4,
                DriverCores = 4,
                Jars =
                {
                    "wasb:///example/jars/azure-cosmosdb-spark-0.0.3-SNAPSHOT.jar",
                    "wasb:///example/jars/azure-documentdb-1.12.0.jar"
                },
                Conf =
                {
                    ["spark.jars.packages"] = "graphframes:graphframes:0.4.0-spark2.0-s_2.11",
                    ["spark.jars.excludes"] = "org.scala-lang:scala-reflect"
                }
            };
    }
}