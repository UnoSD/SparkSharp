using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SparkSharp
{
    public class AsyncLimitedObjectPool<T>
    {
        readonly Func<Task<T>> _factory;
        readonly int _maxObjects;
        readonly ConcurrentQueue<T> _availableObjects = new ConcurrentQueue<T>();
        readonly ConcurrentQueue<TaskCompletionSource<T>> _availablePromises = new ConcurrentQueue<TaskCompletionSource<T>>();
        int _rented;

        /// <param name="factory">Object async factory</param>
        /// <param name="maxObjects">Max number of objects to create (will not be exact)</param>
        public AsyncLimitedObjectPool(Func<Task<T>> factory, int maxObjects) : this(p => factory, maxObjects) { }

        protected AsyncLimitedObjectPool(Func<AsyncLimitedObjectPool<T>, Func<Task<T>>> factory, int maxObjects)
        {
            _factory = factory(this);
            _maxObjects = maxObjects > 0 ? maxObjects : throw new ArgumentException("Argument must be positive non-zero", nameof(maxObjects));
        }

        public ValueTask<T> RentAsync()
        {
            if (_availableObjects.TryDequeue(out var obj))
                return new ValueTask<T>(obj);

            if (_rented >= _maxObjects)
            {
                var promise = new TaskCompletionSource<T>();

                _availablePromises.Enqueue(promise);

                return new ValueTask<T>(promise.Task);
            }

            Thread.MemoryBarrier();

            Interlocked.Increment(ref _rented);

            return new ValueTask<T>(_factory());
        }

        public void Return(T obj)
        {
            if(_availablePromises.TryDequeue(out var promise))
                promise.SetResult(obj);
            
            _availableObjects.Enqueue(obj);
        }
    }
}