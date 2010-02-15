using System.IO;
using FluentNHibernate.Cfg.Db;

namespace Orchard.Data.Migrations {
    public class SQLiteDatabaseCoordinator : DatabaseCoordinatorBase {
        private readonly string _dataFolder;
        private readonly string _connectionString;

        public SQLiteDatabaseCoordinator(string dataFolder, string connectionString) {
            _dataFolder = dataFolder;
            _connectionString = connectionString;
        }

        protected override IPersistenceConfigurer GetPersistenceConfigurer() {
            var persistence = SQLiteConfiguration.Standard;
            if (string.IsNullOrEmpty(_connectionString)) {
                
                if (!Directory.Exists(_dataFolder))
                    Directory.CreateDirectory(_dataFolder);

                persistence = persistence.UsingFile(Path.Combine(_dataFolder, "Orchard.db"));
            }
            else {
                persistence = persistence.ConnectionString(_connectionString);
            }
            return persistence;
        }

    }
}