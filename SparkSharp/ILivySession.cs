using System;
using System.Threading.Tasks;

namespace SparkSharp
{
    interface ILivySession : IDisposable
    {
        Task<T> ExecuteStatementAsync<T>(string code);
        Task WaitForSessionAsync();
    }
}