using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SparkSharp
{
    public class LivyClient : ILivyClient
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

            Logger.Trace($"[{config.Name}] Creating session...");

            var response = await _client.Value.PostAsync("sessions", config).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var sessionPath = response.Headers.Location.AsRelativePath();

            return new LivySession(_client.Value, config, sessionPath);
        }

        public async Task<IEnumerable<ILivySession>> GetSessionsAsync()
        {
            var response = await _client.Value.GetStringAsync("sessions").ConfigureAwait(false);

            var sessions =
                JObject.Parse(response)["sessions"]
                       .Select(jObject => new LivySession(_client.Value, ConfigurationFromJson(jObject), $"sessions/{jObject["id"]}"));

            return sessions;
        }

        static LivySessionConfiguration ConfigurationFromJson(JToken jObject) =>
            new LivySessionConfiguration
            {
                Name = jObject["appId"].ToString(),
                ApplicationId = jObject["appId"].ToString(),
                Kind = (LivySessionKind)Enum.Parse(typeof(LivySessionKind), jObject["kind"].ToString(), true),
                Url = jObject["appInfo"]?["sparkUiUrl"]?.ToString()
            };
    }
}