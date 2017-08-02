using System;
using System.Threading.Tasks;

namespace SparkSharp
{
    // TODO: Please change name to something not ridicolously bad like this
    public class AsyncLimitedDisposableDisposableObjectPool<T> : AsyncLimitedDisposableObjectPool<T>, IDisposable where T : IDisposable
    {
        protected AsyncLimitedDisposableDisposableObjectPool(Func<Task<T>> factory, int max) : base(factory, max) { }

        public void Dispose()
        {
            foreach (var availableObject in AvailableObjects)
                availableObject.Value.Dispose();
        }
    }
}