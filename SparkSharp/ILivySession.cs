using System;
using System.Threading.Tasks;

namespace SparkSharp
{
    public interface ILivySession : IDisposable
    {
        Task<T> ExecuteStatementAsync<T>(string code);
        Task<T> ExecuteStatementAsync<T>(string code, bool silently);
        Task WaitForSessionAsync();
        Task CloseAsync();
        Task<string> GetSessionStateAsync();
    }
}