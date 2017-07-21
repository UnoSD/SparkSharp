using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SparkSharp
{
    public class AsyncObjectPool<T>
    {
        readonly Func<Task<T>> _factory;
        readonly ConcurrentQueue<T> _availableObjects = new ConcurrentQueue<T>();

        public AsyncObjectPool(Func<Task<T>> factory) => _factory = factory;

        public ValueTask<T> RentAsync() =>
                _availableObjects.TryDequeue(out var obj) ?
                                  new ValueTask<T>(obj) :
                                  new ValueTask<T>(_factory());

        public void Return(T obj) => _availableObjects.Enqueue(obj);
    }
}