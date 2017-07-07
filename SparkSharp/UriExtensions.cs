using System;

namespace SparkSharp
{
    static class UriExtensions
    {
        // TODO: Make it non-crap magic 1 number substring
        // Remove leading /
        public static string AsRelativePath(this Uri uri) => uri.ToString().Substring(1);
    }
}