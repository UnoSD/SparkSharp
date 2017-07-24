using System.Collections.Generic;
using System.Threading.Tasks;

namespace SparkSharp
{
    partial class CosmosDbLivySessionPool
    {
        class CosmosDbLivyPooledSession : ISparkSqlSession
        {
            readonly CosmosDbLivySessionPool _pool;

            public CosmosDbLivyPooledSession(CosmosDbLivySessionPool pool) => _pool = pool;

            public Task<IEnumerable<T>> QuerySparkSqlAsync<T>(string sql)
            {
                var tcs = new TaskCompletionSource<IEnumerable<T>>();

                async Task StartJob(CosmosDbLivySession session)
                {
                    var result = await session.QuerySparkSqlAsync<T>(sql).ConfigureAwait(false);

                    tcs.SetResult(result);
                }

                _pool._jobs.Enqueue(StartJob);

                return tcs.Task;
            }
        }
    }
}