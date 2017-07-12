namespace SparkSharp
{
    class SimpleExampleSessionConfiguration
    {
        internal static LivySessionConfiguration GetConfiguration() =>
            new LivySessionConfiguration
            {
                Kind = LivySessionKind.Spark,
                Name = "SparkSharp testing",
                ExecutorMemory = "11G",
                ExecutorCores = 15,
                NumExecutors = 4,
                DriverCores = 4,
            };
    }
}