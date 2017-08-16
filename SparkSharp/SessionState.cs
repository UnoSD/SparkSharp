namespace SparkSharp
{
    public enum SessionState
    {
        Unknown = 0,
        Available,
        Idle,
        Dead,
        Error,
        Starting,
        Waiting,
        Running,
        Busy,
        ShuttingDown
    }
}