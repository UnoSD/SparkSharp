using System;
using System.Threading.Tasks;

namespace SparkSharp
{
    public interface ILivySession : IDisposable
    {
        Task<T> ExecuteStatementAsync<T>(string code);
        Task WaitForSessionAsync();
    }
}