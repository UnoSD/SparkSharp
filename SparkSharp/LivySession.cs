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

        public Task CloseAsync() => _client.DeleteAsync(_sessionPath);

        public void Dispose() => CloseAsync().Wait();

        public Task<T> ExecuteStatementAsync<T>(string code) => ExecuteStatementAsync<T>(code, false);

        public async Task<T> ExecuteStatementAsync<T>(string code, bool silently)
        {
            if(!silently)
                Logger.Trace("Waiting for session to be ready...");

            await WaitForSessionAsync().ConfigureAwait(false);

            if (!silently)
                Logger.Trace("Session ready");

            if (!silently)
                Logger.Trace("Running code...");

            var response = await _client.PostAsync($"{_sessionPath}/statements", new { code })
                                        .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var resultPollingRelativePath = response.Headers.Location.AsRelativePath();

            if (!silently)
                Logger.Trace("Waiting for results to be ready...");

            var result = await WaitForStateAsync(resultPollingRelativePath, "available").ConfigureAwait(false);
            var output = result["output"];

            ThrowIfError(output);

            var data = output["data"]["text/plain"].ToString();

            if (!silently)
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

                if (attempt == 200)
                {
                    Logger.Trace($"Failed to get the session into desired state {expectedState} after 20 seconds, current status: {state}");

                    attempt = 0;
                }

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