using System.Collections.Generic;
using System.Linq;

namespace Orchard.DataMigration {
    public class DefaultDataMigrationGenerator : IDataMigrationGenerator {
        public IEnumerable<IDataMigrationCommand> CreateCommands() {
            return Enumerable.Empty<IDataMigrationCommand>();
        }
    }
}
