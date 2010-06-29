using System.Collections.Generic;
using System.Linq;
using Orchard.DataMigration.Schema;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.DataMigration {
    public class DefaultDataMigrationGenerator : IDataMigrationGenerator {
        public IEnumerable<ISchemaBuilderCommand> CreateCommands(IEnumerable<RecordBlueprint> records) {

            
            return Enumerable.Empty<ISchemaBuilderCommand>();
        }
    }
}
