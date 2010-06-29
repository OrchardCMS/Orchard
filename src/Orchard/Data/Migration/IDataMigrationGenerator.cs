using System.Collections.Generic;
using FluentNHibernate.Cfg;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.Data.Migration {
    // Builds and runs the representative migration create calls
    public interface IDataMigrationGenerator : IDependency {
        IEnumerable<ISchemaBuilderCommand> CreateCommands(IEnumerable<RecordBlueprint> records);
    }
}
