using Orchard.DataMigration.Schema;

namespace Orchard.DataMigration.Interpreters {
    /// <summary>
    /// This interface can be implemented to provide a data migration behavior
    /// </summary>
    public interface ICommandInterpreter<in T> : ICommandInterpreter
    where T : ISchemaBuilderCommand {
        string[] CreateStatements(T command);
    }

    public interface ICommandInterpreter : IDependency {
        string DataProvider { get; }
    }
}
