using System.Collections.Generic;
using System.Linq;
using Orchard.Data.Migration.Schema;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.Data.Migration {
    public class DefaultDataMigrationGenerator : IDataMigrationGenerator {
        public IEnumerable<ISchemaBuilderCommand> CreateCommands(IEnumerable<RecordBlueprint> records) {

            
            return Enumerable.Empty<ISchemaBuilderCommand>();
        }
    }
}
