using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SparkSharp
{
    static class EnumerableExtensions
    {
        internal static Task<T> FirstCompletedAsync<T>(this IEnumerable<T> source, Func<T, Task> taskToComplete) =>
                Task.WhenAny(source.Select(async item =>
                {
                    await taskToComplete(item);

                    return item;
                })).Unwrap();

        internal static IEnumerable<string> AsStrings<T>(this IEnumerable<T> source) => source.Select(item => item.ToString());

        internal static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>
            (this IEnumerable<ValueTask<TSource>> sourceTasks, Func<TSource, Task<TResult>> selector)
        {
            // TODO: Task.WhenAll(ValueTask)
            var source = await Task.WhenAll(sourceTasks.Select(t => t.AsTask())).ConfigureAwait(false);

            return await Task.WhenAll(source.Select(selector)).ConfigureAwait(false);
        }
    }
}