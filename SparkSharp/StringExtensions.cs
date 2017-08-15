using System;
using System.Collections.Generic;

namespace SparkSharp
{
    static class StringExtensions
    {
        internal static bool AsBooleanOrDefault(this string source, bool defaultValue) =>
            bool.TryParse(source, out var result) ? result : defaultValue;

        internal static string Join(this IEnumerable<string> strings) => strings.Join(Environment.NewLine);

        internal static string Join(this IEnumerable<string> strings, string separator) => string.Join(separator, strings);
    }
}