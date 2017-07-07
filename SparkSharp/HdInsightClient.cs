namespace SparkSharp
{
    class HdInsightClient : LivyClient
    {
        public HdInsightClient(string clusterName, string username, string password) :
            base(GetLivyUrl(clusterName), username, password) { }

        static string GetLivyUrl(string clusterName) => $"https://{clusterName}.azurehdinsight.net/livy";
    }
}