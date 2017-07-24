using System.Configuration;

namespace SparkSharp
{
    static class Program
    {
        static void Main()
        {
            ApplicationSettings.SetupSettings();

            var settings = ConfigurationManager.AppSettings;

            switch (settings["AutoStartExample"])
            {
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
