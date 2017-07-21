using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SparkSharp
{
    static class ObjectExtensions
    {
        static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static string ToJsonString(this object value) => 
            JsonConvert.SerializeObject(value, JsonSerializerSettings);

        // TODO: Replace this hacky cloning
        public static T Clone<T>(this T obj) => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
    }
}