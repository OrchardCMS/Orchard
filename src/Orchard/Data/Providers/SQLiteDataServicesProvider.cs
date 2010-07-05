using System.IO;
using FluentNHibernate.Cfg.Db;

namespace Orchard.Data.Providers {    
    public class SQLiteDataServicesProvider : AbstractDataServicesProvider {
        private readonly string _dataFolder;
        private readonly string _connectionString;

        public SQLiteDataServicesProvider(string dataFolder, string connectionString) {
            _dataFolder = dataFolder;
            _connectionString = connectionString;
        }

        public static string ProviderName {
            get { return "SQLite"; }
        }

        public override IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase) {
            var persistence = SQLiteConfiguration.Standard;
            if (string.IsNullOrEmpty(_connectionString)) {
                var dataFile = Path.Combine(_dataFolder, "Orchard.db");

                if (!Directory.Exists(_dataFolder)) {
                    Directory.CreateDirectory(_dataFolder);
                }
                
                if (createDatabase && File.Exists(dataFile)) {
                    File.Delete(dataFile);
                }

                persistence = persistence.UsingFile(dataFile);
            }
            else {
                persistence = persistence.ConnectionString(_connectionString);
            }
            return persistence;
        }
    }
}
