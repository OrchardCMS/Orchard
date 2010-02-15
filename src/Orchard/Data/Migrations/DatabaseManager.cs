using System;

namespace Orchard.Data.Migrations {
    public class DatabaseManager : IDatabaseManager {
        public ISessionFactoryBuilder CreateCoordinator(DatabaseParameters databaseParameters) {
            if (string.Equals(databaseParameters.Provider, "SQLite", StringComparison.InvariantCultureIgnoreCase))
                return new SQLiteSessionFactoryBuilder(databaseParameters.DataFolder, databaseParameters.ConnectionString);
            return new SqlServerSessionFactoryBuilder(databaseParameters.DataFolder, databaseParameters.ConnectionString);
        }
    }
}
