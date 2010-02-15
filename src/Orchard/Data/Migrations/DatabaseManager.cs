using System;

namespace Orchard.Data.Migrations {
    public class DatabaseManager : IDatabaseManager {
        public ISessionFactoryBuilder CreateCoordinator(string provider, string dataFolder, string connectionString) {
            if (string.Equals(provider, "SQLite", StringComparison.InvariantCultureIgnoreCase))
                return new SQLiteSessionFactoryBuilder(dataFolder, connectionString);
            return new SqlServerSessionFactoryBuilder(dataFolder, connectionString);
        }
    }
}
