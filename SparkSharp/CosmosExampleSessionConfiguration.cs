namespace SparkSharp
{
    class CosmosExampleSessionConfiguration
    {
        internal static LivySessionConfiguration GetConfiguration() => 
            new LivySessionConfiguration
            {
                Kind = LivySessionKind.Spark,
                Name = "SparkSharp testing",
                ExecutorMemory = "1G",
                ExecutorCores = 2,
                NumExecutors = 2,
                DriverCores = 2,
                Jars =
                {
                    "hdfs://master.hadoop.lan:9000/azure-cosmosdb-spark-0.0.3-SNAPSHOT.jar",
                    "hdfs://master.hadoop.lan:9000/azure-documentdb-1.12.0.jar"
                },
                Conf =
                {
                    ["spark.jars.packages"] = "graphframes:graphframes:0.4.0-spark2.0-s_2.11",
                    ["spark.jars.excludes"] = "org.scala-lang:scala-reflect"
                }
            };
    }
}