using System;

namespace Orchard.Data.Migrations {
    public class DatabaseMigrationManager : IDatabaseMigrationManager {
        public IDatabaseCoordinator CreateCoordinator(string provider, string dataFolder, string connectionString) {
            if (string.Equals(provider, "SQLite", StringComparison.InvariantCultureIgnoreCase))
                return new SQLiteDatabaseCoordinator(dataFolder, connectionString);
            return new SqlServerDatabaseCoordinator(dataFolder, connectionString);
        }
    }
}
