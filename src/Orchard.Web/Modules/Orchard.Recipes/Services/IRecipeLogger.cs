using Orchard.Logging;

namespace Orchard.Recipes.Services {
    public interface IRecipeLoggerFactory : IDependency {
        IRecipeLogger CreateLogger(string executionId);
    }

    public interface IRecipeLogger : ILogger {
        string ExecutionId { get; }
    }

    public class RecipeLogerFactory : IRecipeLoggerFactory {
        public IRecipeLogger CreateLogger(string executionId) {
            return new FakeRecipeLogger(executionId);
        }
    }

    public class FakeRecipeLogger : NullLogger, IRecipeLogger {
        public FakeRecipeLogger(string executionId) {
            ExecutionId = executionId;
        }

        public string ExecutionId { get; private set; }
    }

    public static class RecipeLoggerExtensions {
        public static void LogExecutionStart(this IRecipeLogger logger) {
            logger.Information("Starting execution with id {0}.", logger.ExecutionId);
        }

        public static void LogExecutionComplete(this IRecipeLogger logger) {
            logger.Information("Completed execution with id {0}.", logger.ExecutionId);
        }

        public static void LogExecutionFailed(this IRecipeLogger logger) {
            logger.Warning("Failed execution with id {0}.", logger.ExecutionId);
        }
    }
}