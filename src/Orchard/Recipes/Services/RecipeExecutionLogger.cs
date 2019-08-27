using System;
using Orchard.Logging;

namespace Orchard.Recipes.Services {
    public class RecipeExecutionLogger : ITransientDependency, ILogger {
        public RecipeExecutionLogger() {
            Logger = NullLogger.Instance;
        }

        public Type ComponentType { get; internal set; }
        public ILogger Logger { get; set; }

        public bool IsEnabled(LogLevel level) {
            return Logger.IsEnabled(level);
        }

        public void Log(LogLevel level, Exception exception, string format, params object[] args) {
            var message = args == null ? format : string.Format(format, args);
            Logger.Log(level, exception, "{0}: {1}", ComponentType.Name, message);
        }
    }
}
