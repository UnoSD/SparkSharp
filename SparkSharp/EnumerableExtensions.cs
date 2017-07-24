using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SparkSharp
{
    static class EnumerableExtensions
    {
        public static Task<T> FirstCompletedAsync<T>(this IEnumerable<T> source, Func<T, Task> taskToComplete) =>
                Task.WhenAny(source.Select(async item =>
                {
                    await taskToComplete(item);

                    return item;
                })).Unwrap();
    }
}