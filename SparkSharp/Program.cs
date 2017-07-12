using System.Configuration;

namespace SparkSharp
{
    static class Program
    {
        static void Main()
        {
            //ApplicationSettings.SetupSettings();

            SimpleExample.ExampleAsync(ConfigurationManager.AppSettings).GetAwaiter().GetResult();
            //CosmosExample.ExampleAsync(ConfigurationManager.AppSettings).GetAwaiter().GetResult();
        }
    }
}
