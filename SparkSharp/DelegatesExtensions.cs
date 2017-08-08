using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SparkSharp
{
    static class DelegatesExtensions
    {
        internal static async Task<(TimeSpan Elapsed, T Result)> RunAndTimeExecution<T>(this Func<Task<T>> action)
        {
            var stopwatch = Stopwatch.StartNew();

            var result = await action().ConfigureAwait(false);

            return (stopwatch.Elapsed, result);
        }
    }
}