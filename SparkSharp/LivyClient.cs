using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SparkSharp
{
    class LivyClient : ILivyClient
    {
        readonly Lazy<HttpClient> _client;

        internal LivyClient(string url, string username, string password) =>
            _client = new Lazy<HttpClient>(() => CreateClient(url, username, password));

        static HttpClient CreateClient(string url, string username, string password)
        {
            var clientHandler = new HttpClientHandler
            {
                Credentials = new NetworkCredential(username, password)
            };

            var client = new HttpClient(clientHandler)
            {
                BaseAddress = new Uri(url.EndsWith("/") ? url : $"{url}/"),
                DefaultRequestHeaders =
                {
                    Accept = { new MediaTypeWithQualityHeaderValue("application/json") }
                }
            };

            return client;
        }

        public void Dispose()
        {
            if (_client.IsValueCreated)
                _client.Value.Dispose();
        }

        public async Task<ILivySession> CreateSessionAsync(LivySessionConfiguration config)
        {
            config = config ?? LivySessionConfiguration.Default;
            
            var response = await _client.Value.PostAsync("sessions", config).ConfigureAwait(false);
            
            response.EnsureSuccessStatusCode();

            var sessionPath = response.Headers.Location.AsRelativePath();

            return new LivySession(_client.Value, config, sessionPath);
        }
    }
}