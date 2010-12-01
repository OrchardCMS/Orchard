using System;

namespace Orchard.Logging {
    public enum LogLevel {
        Debug,
        Information,
        Warning,
        Error,
        Fatal
    }

    public interface ILogger : ISingletonDependency {
        bool IsEnabled(LogLevel level);
        void Log(LogLevel level, Exception exception, string format, params object[] args);
    }
}
