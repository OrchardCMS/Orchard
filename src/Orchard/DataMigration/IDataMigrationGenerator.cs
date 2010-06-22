using System.Collections.Generic;
using Orchard.DataMigration.Commands;

namespace Orchard.DataMigration {
    // Builds and runs the representative migration create calls
    public interface IDataMigrationGenerator : IDependency {
        IEnumerable<IDataMigrationCommand> CreateCommands();
    }
}
