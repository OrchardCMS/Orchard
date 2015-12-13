using System;

namespace Orchard.Logging {
    public interface ILoggerFactory {
        ILogger CreateLogger(Type type);
    }
}