using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SparkSharp
{
    public interface ILivySession : IDisposable
    {
        Task<IEnumerable<T>> ExecuteStatementAsync<T>(string code);
        Task WaitForSessionAsync();
    }
}