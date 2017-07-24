namespace SparkSharp
{
    static class StringExtensions
    {
        internal static bool AsBooleanOrDefault(this string source, bool defaultValue) =>
            bool.TryParse(source, out var result) ? result : defaultValue;
    }
}