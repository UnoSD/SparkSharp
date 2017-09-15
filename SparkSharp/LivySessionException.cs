using System;

namespace SparkSharp
{
    public class LivySessionException : Exception
    {
        public string SessionName { get; }

        internal LivySessionException(string sessionName, string message) : base(message) =>
            SessionName = sessionName;
    }
}