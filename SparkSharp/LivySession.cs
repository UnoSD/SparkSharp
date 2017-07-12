using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SparkSharp
{
    class LivySession : ILivySession
    {
        readonly HttpClient _client;
        readonly LivySessionConfiguration _config;
        readonly string _sessionPath;

        // TODO: Replace this hacky cloning
        public LivySessionConfiguration Configuration =>
            JsonConvert.DeserializeObject<LivySessionConfiguration>(JsonConvert.SerializeObject(_config));

        public LivySession(HttpClient client, LivySessionConfiguration config, string sessionPath)
        {
            _client = client;
            _config = config;
            _sessionPath = sessionPath;
        }

        // TODO: Check success without throwing exception (retry)
#if DEBUG
        public async Task CloseAsync()
        {
            var response = await _client.DeleteAsync(_sessionPath).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
        }
#else
        public Task CloseAsync() => _client.DeleteAsync(_sessionPath);
#endif

        public void Dispose() => CloseAsync().Wait();

        public async Task<T> ExecuteStatementAsync<T>(string code)
        {
            Logger.Trace("Waiting for session to be ready...");

            await WaitForSessionAsync().ConfigureAwait(false);

            Logger.Trace("Session ready");

            Logger.Trace("Running code...");

            var response = await _client.PostAsync($"{_sessionPath}/statements", new { code })
                                        .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var resultPollingRelativePath = response.Headers.Location.AsRelativePath();

            Logger.Trace("Waiting for results to be ready...");

            var result = await WaitForStateAsync(resultPollingRelativePath, "available").ConfigureAwait(false);
            var data = result["output"]["data"]["text/plain"].ToString();

            Logger.Trace("Results ready");

            return JsonConvert.DeserializeObject<T>(data);
        }

        public Task WaitForSessionAsync() => WaitForStateAsync(_sessionPath, "idle");

        async Task<JObject> WaitForStateAsync(string pollingUri, string expectedState)
        {
            while (true)
            {
                var message = new HttpRequestMessage(HttpMethod.Get, pollingUri);
                message.Headers.Accept.Clear();
                message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await _client.SendAsync(message).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    continue;

                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var jObject = JObject.Parse(result);
                var state = jObject["state"].ToString();
                
                if (state == expectedState)
                    return jObject;

                // TODO: Decide a reasonable configurable delay
                await Task.Delay(100).ConfigureAwait(false);
            }
        }
    }
}