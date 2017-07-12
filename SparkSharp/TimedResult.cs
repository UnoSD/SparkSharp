using System;

namespace SparkSharp
{
    public class TimedResult<T>
    {
        public T Result { get; set; }
        public TimeSpan Elapsed { get; set; }
    }
}