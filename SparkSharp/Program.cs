using System;
using System.Configuration;

namespace SparkSharp
{
    static class Program
    {
        static void Main()
        {
            ApplicationSettings.SetupSettings();

            var settings = ConfigurationManager.AppSettings;

            SimpleExample.ExampleAsync(settings).GetAwaiter().GetResult();
            CosmosExample.ExampleAsync(settings).GetAwaiter().GetResult();
            CosmosBaseQueryExample.ExampleAsync(settings).GetAwaiter().GetResult();
            CosmosPoolingExample.ExampleAsync(settings).GetAwaiter().GetResult();
            CosmosPoolingBaseQueryExample.ExampleAsync(settings).GetAwaiter().GetResult();
            Console.WriteLine("DONE");
            Console.ReadKey();

            Console.WriteLine(settings["AutoStartExample"]);

            switch (settings["AutoStartExample"])
            {
                case nameof(CosmosBaseQueryExample):
                    CosmosBaseQueryExample.ExampleAsync(settings).GetAwaiter().GetResult();
                    break;
                case nameof(CosmosPoolingExample):
                    CosmosPoolingExample.ExampleAsync(settings).GetAwaiter().GetResult();
                    break;
                case nameof(CosmosPoolingBaseQueryExample):
                    CosmosPoolingBaseQueryExample.ExampleAsync(settings).GetAwaiter().GetResult();
                    break;
                case nameof(CosmosExample):
                    CosmosExample.ExampleAsync(settings).GetAwaiter().GetResult();
                    break;
                default:
                    SimpleExample.ExampleAsync(settings).GetAwaiter().GetResult();
                    break;
            }
        }
    }
}
