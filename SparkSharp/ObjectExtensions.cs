using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SparkSharp
{
    static class ObjectExtensions
    {
        static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        internal static string ToJsonString(this object value) => 
            JsonConvert.SerializeObject(value, JsonSerializerSettings);

        // TODO: Replace this hacky cloning
        internal static T Clone<T>(this T obj) => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));

        internal static bool In<T>(this T value, params T[] list) => list.Contains(value);
    }
}