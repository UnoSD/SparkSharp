using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SparkSharp
{
    static class TaskExtensions
    {
        internal static TaskAwaiter<T[]> GetAwaiter<T>(this IEnumerable<Task<T>> tasks) => Task.WhenAll(tasks).GetAwaiter();
        internal static TaskAwaiter GetAwaiter(this IEnumerable<Task> tasks) => Task.WhenAll(tasks).GetAwaiter();
        internal static TaskAwaiter<T[]> GetAwaiter<T>(this IEnumerable<ValueTask<T>> tasks) => Task.WhenAll(tasks.Select(t => t.AsTask())).GetAwaiter();
    }
}