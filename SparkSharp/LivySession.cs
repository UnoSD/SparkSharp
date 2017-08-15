using System;
using System.Linq;
using System.Net.Http;
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

        public LivySession(HttpClient client, LivySessionConfiguration config, string sessionPath)
        {
            _client = client;
            _config = config;
            _sessionPath = sessionPath;
        }

        public Task CloseAsync() => _client.DeleteAsync(_sessionPath);

        public void Dispose()
        {
            try
            {
                CloseAsync().GetAwaiter().GetResult();
            }
            catch { /**/ }
        }

        public Task<T> ExecuteStatementAsync<T>(string code) => ExecuteStatementAsync<T>(code, false);

        async Task<T> ExecuteStatementAsync<T>(string code, bool silently)
        {
            if (!silently)
                Log("Waiting for session to be ready...");

            await WaitForSessionAsync().ConfigureAwait(false);

            if (!silently)
                Log("Session ready");

            if (!silently)
                Log("Running code...");

            var response = await _client.PostAsync($"{_sessionPath}/statements", new { code })
                                        .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                Log(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

            response.EnsureSuccessStatusCode();

            var resultPollingRelativePath = response.Headers.Location.AsRelativePath();

            if (!silently)
                Log("Waiting for results to be ready...");

            var result = await WaitForStatesAsync(resultPollingRelativePath, "available").ConfigureAwait(false);
            var output = result["output"];

            ThrowIfError(output);

            var data = output["data"]["text/plain"].ToString();

            if (!silently)
                Log("Results ready");

            return JsonConvert.DeserializeObject<T>(data);
        }

        static void ThrowIfError(JToken output)
        {
            var error = output["status"].ToString() == "error";

            if (error)
                throw new Exception($"{output["evalue"]}\n\n{output["traceback"]}");
        }

        public async Task WaitForSessionAsync()
        {
            var result = await WaitForStatesAsync(_sessionPath, "idle", "error", "dead", "shutting_down").ConfigureAwait(false);

            if (result["state"].ToString() != "idle")
                throw new Exception(result.ToString());
        }

        async Task<JObject> WaitForStatesAsync(string pollingUri, params string[] expectedStates)
        {
            for (var attempt = 0; ; attempt++)
            {
                var jObject = await GetResultAsync(pollingUri).ConfigureAwait(false);

                var state = jObject["state"].ToString();

                if (expectedStates.Contains(state))
                    return jObject;

                if (attempt == 300)
                {
                    Log($"Failed to get the session into desired state {expectedStates.Join(", ")} after 30 seconds, current status: {state}");

                    attempt = 0;
                }

                // TODO: Decide a reasonable configurable delay
                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        async Task<JObject> GetResultAsync(string uri)
        {
            var response = await _client.GetAsync(uri).ConfigureAwait(false);
            
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JObject.Parse(result);
        }

        public async Task<SessionState> GetSessionStateAsync()
        {
            var jObject = await GetResultAsync(_sessionPath).ConfigureAwait(false);

            var state = (SessionState)Enum.Parse(typeof(SessionState), jObject["state"].ToString(), true);

            return state;
        }

        void Log(string message) => Logger.Trace($"[{_config.Name}] {message}");
    }
}