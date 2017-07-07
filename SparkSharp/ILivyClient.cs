using System;
using System.Threading.Tasks;

namespace SparkSharp
{
    interface ILivyClient : IDisposable
    {
        Task<ILivySession> CreateSessionAsync(LivySessionConfiguration config = default(LivySessionConfiguration));
    }
}