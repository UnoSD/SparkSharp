# SparkSharp
C# Livy client to submit Spark jobs to HDInsight and other Spark clusters

It contains also a snippet to run Spark SQL on Cosmos DB and return the results

Example usages:

Simple

```csharp
using (var client = new LivyClient("http://url-to-livy", "username", "password"))
using (var session = await client.CreateSessionAsync(SimpleExampleSessionConfiguration.GetConfiguration()))
{
    var sum = await session.ExecuteStatementAsync<int>("val res = 1 + 1\nprintln(res)");
    
    // Prints 2
    Console.WriteLine(sum);
}
````

Cosmos DB Spark SQL

```csharp
var cosmosSettings = new CosmosCollectionSettings
{
    Name = "CosmosName",
    Key = "CosmosKey",
    Database = "CosmosDatabase",
    Collection = "CosmosCollection",
    PreferredRegions = "CosmosPreferredRegions"
};

using (var client = new HdInsightClient("clusterName", "admin", "password"))
using (var cosmos = new CosmosDbLivySession(client, cosmosSettings, CosmosExampleSessionConfiguration.GetConfiguration()))
{
    // Group by on Cosmos, yeah!
    const string sql = "SELECT id, SUM(json.total) AS total FROM cosmos GROUP BY id";

    var results = await cosmos.QuerySparkSqlAsync<Result>(sql);
                
    // Prints all the records resulting from the query and mapped to Result
    results.ToList().ForEach(t => Console.WriteLine($"{t.ContactIdentifier}:{t.Count}"));
}
```

Cosmos DB connector for Spark jars available here (with the guide in the wiki on how to set it up in HDInsight): https://github.com/Azure/azure-cosmosdb-spark/tree/master/releases

On exceptions, kill the dangling session from here: https://\<yourHdInsightClusterName\>.azurehdinsight.net/yarnui/hn/cluster/apps/RUNNING
