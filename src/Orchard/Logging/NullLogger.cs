using System;

namespace Orchard.Logging
{
    public class NullLogger : ILogger
    {
        public static ILogger Instance { get; } = new NullLogger();

        public bool IsEnabled(LogLevel level)
        {
            return false;
        }

        public void Log(LogLevel level, Exception exception, string format, params object[] args)
        {
        }
    }
}