using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SparkSharp
{
    class LivySessionConfiguration
    {
        public static LivySessionConfiguration Default { get; } = new LivySessionConfiguration
        {
            Kind = LivySessionKind.Spark
        };

        [JsonConverter(typeof(StringEnumConverter), true)]
        public LivySessionKind Kind { get; set; }
        public string Name { get; set; }
        public string ExecutorMemory { get; set; }
        public int ExecutorCores { get; set; }
        public int NumExecutors { get; set; }
        public int DriverCores { get; set; }
        public IList<string> Jars { get; } = new List<string>();
        public IDictionary<string, string> Conf { get; } = new Dictionary<string, string>();
    }
}