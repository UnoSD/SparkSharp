using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SparkSharp
{
    public class CosmosDbLivySession : IDisposable
    {
        readonly CosmosCollectionSettings _settings;
        readonly Lazy<Task<ILivySession>> _session;

        public CosmosDbLivySession(ILivyClient client, CosmosCollectionSettings settings, LivySessionConfiguration livyConfig)
        {
            _settings = settings;
            _session = new Lazy<Task<ILivySession>>(() => client.CreateSessionAsync(livyConfig));
        }

        /// <summary>
        /// Use cosmos as source.
        /// Example: SELECT * FROM cosmos
        /// </summary>
        public async Task<TimedResult<IEnumerable<T>>> QuerySparkSqlWithMetricsAsync<T>(string sql)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var results = await QuerySparkSqlAsync<T>(sql).ConfigureAwait(false);
            
            return new TimedResult<IEnumerable<T>> { Result = results, Elapsed = stopwatch.Elapsed };
        }

        /// <summary>
        /// Use cosmos as source.
        /// Example: SELECT * FROM cosmos
        /// </summary>
        public async Task<IEnumerable<T>> QuerySparkSqlAsync<T>(string sql)
        {
            var initializeContextCode = _session.IsValueCreated ?
                                        string.Empty :
                                        GetInitializeContextCode();

            var session = await _session.Value.ConfigureAwait(false);

            var scalaCode = $@"
{initializeContextCode}

val docs = spark.sql(""{sql}"")
docs.createOrReplaceTempView(""docs"")

println(docs.toJSON.collect.mkString(""["", "","", ""]""))
";

            return await session.ExecuteStatementAsync<IEnumerable<T>>(scalaCode).ConfigureAwait(false);
        }

        string GetInitializeContextCode() => $@"
import com.microsoft.azure.cosmosdb.spark.schema._
import com.microsoft.azure.cosmosdb.spark._
import com.microsoft.azure.cosmosdb.spark.config.Config

val config = Config(Map(""Endpoint""         -> ""https://{_settings.Name}.documents.azure.com:443/"",
                        ""Masterkey""        -> ""{_settings.Key}"",
                        ""Database""         -> ""{_settings.Database}"",
                        ""preferredRegions"" -> ""{_settings.PreferredRegions};"",
                        ""Collection""       -> ""{_settings.Collection}"", 
                        ""SamplingRatio""    -> ""1.0""))

val collection = spark.sqlContext.read.cosmosDB(config)
collection.createOrReplaceTempView(""cosmos"")
";

        public void Dispose()
        {
            if (_session.IsValueCreated)
                _session.Value.Result.Dispose();
        }
    }
}