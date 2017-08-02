using System;
using System.Threading.Tasks;

namespace SparkSharp
{
    public class AsyncLimitedDisposableObjectPool<T> : AsyncLimitedObjectPool<AsyncLimitedDisposableObjectPool<T>.PoolObject>
    {
        public class PoolObject : IDisposable
        {
            public T Value { get; }
            readonly AsyncLimitedObjectPool<PoolObject> _pool;

            internal PoolObject(AsyncLimitedObjectPool<PoolObject> pool, T value)
            {
                _pool = pool;
                Value = value;
            }

            public void Dispose() => _pool.Return(this);
        }

        protected AsyncLimitedDisposableObjectPool(Func<Task<T>> factory, int max) : base(@this => (() => GetFactory(factory, @this)), max) { }

        static async Task<PoolObject> GetFactory(Func<Task<T>> factory, AsyncLimitedObjectPool<PoolObject> a)
        {
            var value = await factory().ConfigureAwait(false);

            return new PoolObject(a, value);
        }
    }
}