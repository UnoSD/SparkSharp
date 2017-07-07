using System.Net.Http;
using System.Threading.Tasks;

namespace SparkSharp
{
    static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PostAsync(this HttpClient client, string url, string body) =>
            client.PostAsync(url, new StringContent(body));

        public static Task<HttpResponseMessage> PostAsync(this HttpClient client, string url, object body) =>
            client.PostAsync(url, body.ToJsonString());
    }
}