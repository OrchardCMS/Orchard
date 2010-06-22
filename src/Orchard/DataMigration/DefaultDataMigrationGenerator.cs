using System.Collections.Generic;
using System.Linq;
using Orchard.DataMigration.Commands;

namespace Orchard.DataMigration {
    public class DefaultDataMigrationGenerator : IDataMigrationGenerator {
        public IEnumerable<IDataMigrationCommand> CreateCommands() {
            return Enumerable.Empty<IDataMigrationCommand>();
        }
    }
}
