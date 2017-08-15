using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SparkSharp
{
    public interface ILivyClient : IDisposable
    {
        Task<ILivySession> CreateSessionAsync(LivySessionConfiguration config = default(LivySessionConfiguration));
        Task<IEnumerable<ILivySession>> GetSessionsAsync();
    }
}