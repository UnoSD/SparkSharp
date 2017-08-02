using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SparkSharp
{
    public class CosmosDbLivySession : IDisposable, ISparkSqlSession
    {
        readonly CosmosCollectionSettings _settings;
        readonly Lazy<Task<ILivySession>> _session;
        static int _sessionCount;

        public CosmosDbLivySession(ILivyClient client, CosmosCollectionSettings settings, LivySessionConfiguration livyConfig)
        {
            _settings = settings;
            _session = new Lazy<Task<ILivySession>>(() => CreateSessionAsync(client, livyConfig));
        }

        async Task<ILivySession> CreateSessionAsync(ILivyClient client, LivySessionConfiguration livyConfig)
        {
            var livySessionConfiguration = livyConfig.Clone();

            livySessionConfiguration.Name += " " + Interlocked.Increment(ref _sessionCount);

            var session = await client.CreateSessionAsync(livySessionConfiguration).ConfigureAwait(false);

            await session.ExecuteStatementAsync<object>(GetInitializeContextCode(), true).ConfigureAwait(false);

            return session;
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

        public async Task WaitForSessionAsync()
        {
            var session = await _session.Value.ConfigureAwait(false);

            await session.WaitForSessionAsync().ConfigureAwait(false);
        }

        public async Task<bool> GetSessionAvailableAsync()
        {
            if (!_session.IsValueCreated)
                return false;

            var session = await _session.Value.ConfigureAwait(false);

            var state = await session.GetSessionStateAsync().ConfigureAwait(false);

            return state == "idle";
        }

        /// <summary>
        /// Use cosmos as source.
        /// Example: SELECT * FROM cosmos
        /// </summary>
        public async Task<IEnumerable<T>> QuerySparkSqlAsync<T>(string sql)
        {
            var session = await _session.Value.ConfigureAwait(false);

            var scalaCode = $@"
val docs = spark.sql(""{sql}"")

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

spark.sqlContext.read.cosmosDB(config).createOrReplaceTempView(""cosmos"")
";

        public void Dispose() => CloseAsync().GetAwaiter().GetResult();

        public async Task CloseAsync()
        {
            if (_session.IsValueCreated)
                await (await _session.Value.ConfigureAwait(false)).CloseAsync().ConfigureAwait(false);
        }
    }
}