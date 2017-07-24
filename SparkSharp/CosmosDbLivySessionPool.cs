using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace SparkSharp
{
    public partial class CosmosDbLivySessionPool : IDisposable
    {
        readonly ILivyClient _client;
        readonly CosmosCollectionSettings _cosmosCollectionSettings;
        readonly LivySessionConfiguration _livySessionConfiguration;
        readonly int _maxSessions;
        readonly ConcurrentBag<CosmosDbLivySession> _sessions = new ConcurrentBag<CosmosDbLivySession>();
        readonly ConcurrentQueue<Func<CosmosDbLivySession, Task>> _jobs = new ConcurrentQueue<Func<CosmosDbLivySession, Task>>();

        public CosmosDbLivySessionPool(ILivyClient client, CosmosCollectionSettings cosmosCollectionSettings, LivySessionConfiguration livySessionConfiguration, int maxSessions)
        {
            _client = client;
            _cosmosCollectionSettings = cosmosCollectionSettings;
            _livySessionConfiguration = livySessionConfiguration;
            _maxSessions = maxSessions;
        }

        async Task<ISparkSqlSession> GetAvailableSessionFromPoolAsync()
        {
            var availableSessions = _sessions.Select(session => new { session, available = session.GetSessionAvailableAsync() })
                                             .ToList();

            while (availableSessions.Any())
            {
                var first = await Task.WhenAny(availableSessions.Select(s => s.available)).ConfigureAwait(false);
                var pair = availableSessions.First(s => s.available.IsCompleted);

                if (await first.ConfigureAwait(false))
                    return pair.session;

                availableSessions.Remove(pair);
            }

            return null;
        }
        
        public async Task<ISparkSqlSession> GetSessionAsync()
        {
            var availableSession = await GetAvailableSessionFromPoolAsync().ConfigureAwait(false);

            if (availableSession != null)
                return availableSession;

            if (_sessions.Count >= _maxSessions)
                return await _sessions.FirstCompletedAsync(t => t.WaitForSessionAsync()).ConfigureAwait(false);

            var session = new CosmosDbLivySession(_client, _cosmosCollectionSettings, _livySessionConfiguration);

            _sessions.Add(session);

            return session;
        }

        public void Dispose() => _sessions.Select(session => session.CloseAsync())
                                          .GetAwaiter()
                                          .GetResult();
    }
}