using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SparkSharp
{
    class LivySession : ILivySession
    {
        static readonly JObject ErrorJObject = new JObject { { "state", "error" } };

        readonly HttpClient _client;
        readonly LivySessionConfiguration _config;
        readonly string _sessionPath;

        public LivySessionConfiguration Configuration => _config.Clone();

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
            var output = result["output"];

            ThrowIfError(output);

            var data = output["data"]["text/plain"].ToString();

            Logger.Trace("Results ready");

            return JsonConvert.DeserializeObject<T>(data);
        }

        static void ThrowIfError(JToken output)
        {
            var error = output["status"].ToString() == "error";

            if (error)
                throw new Exception($"{output["evalue"]}\n\n{output["traceback"]}");
        }

        public Task WaitForSessionAsync() => WaitForStateAsync(_sessionPath, "idle");

        async Task<JObject> WaitForStateAsync(string pollingUri, string expectedState)
        {
            for (var attempt = 0; ; attempt++)
            {
                var jObject = await GetResultAsync(pollingUri).ConfigureAwait(false);
                var state = jObject["state"].ToString();

                if (attempt == 600)
                    Logger.Trace($"Failed to get a session after 60 seconds, current status: {state}");

                if (state == expectedState)
                    return jObject;

                // TODO: Decide a reasonable configurable delay
                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        async Task<JObject> GetResultAsync(string uri)
        {
            var response = await _client.GetAsync(uri).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                return ErrorJObject;

            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JObject.Parse(result);
        }

        public async Task<string> GetSessionStateAsync()
        {
            var jObject = await GetResultAsync(_sessionPath).ConfigureAwait(false);

            var state = jObject["state"].ToString();

            return state;
        }
    }
}