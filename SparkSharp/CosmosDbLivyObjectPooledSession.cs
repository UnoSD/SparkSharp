using System.Collections.Generic;
using System.Threading.Tasks;

namespace SparkSharp
{
    public class CosmosDbLivyObjectPooledSession : AsyncLimitedDisposableObjectPool<CosmosDbLivySession>, ISparkSqlSession
    {
        public CosmosDbLivyObjectPooledSession(ILivyClient client, CosmosCollectionSettings settings, LivySessionConfiguration config, int max) :
            base(() => CreateSession(client, settings, config), max)
        { }

        static async Task<CosmosDbLivySession> CreateSession(ILivyClient client, CosmosCollectionSettings cosmosCollectionSettings, LivySessionConfiguration livySessionConfiguration)
        {
            var session = new CosmosDbLivySession(client, cosmosCollectionSettings, livySessionConfiguration);

            await session.WaitForSessionAsync().ConfigureAwait(false);

            return session;
        }

        public async Task<IEnumerable<T>> QuerySparkSqlAsync<T>(string sql)
        {
            using (var session = await RentAsync().ConfigureAwait(false))
                return await session.Value.QuerySparkSqlAsync<T>(sql).ConfigureAwait(false);
        }
    }
}