using System.Collections.Generic;
using FluentNHibernate.Cfg;
using Orchard.DataMigration.Schema;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.DataMigration {
    // Builds and runs the representative migration create calls
    public interface IDataMigrationGenerator : IDependency {
        IEnumerable<ISchemaBuilderCommand> CreateCommands(IEnumerable<RecordBlueprint> records);
    }
}
