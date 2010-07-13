using System;
using System.IO;
using FluentNHibernate.Cfg.Db;

namespace Orchard.Data.Providers {
    public class SqlCeDataServicesProvider : AbstractDataServicesProvider {
        private readonly string _dataFolder;
        private readonly string _connectionString;

        public SqlCeDataServicesProvider(string dataFolder, string connectionString) {
            _dataFolder = dataFolder;
            _connectionString = connectionString;
        }

        public static string ProviderName {
            get { return "SqlCe"; }
        }

        public override IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase) {
            var persistence = MsSqlCeConfiguration.Standard;

            if (string.IsNullOrEmpty(_connectionString)) {
                var dataFile = Path.Combine(_dataFolder, "Orchard.sdf");

                if (!Directory.Exists(_dataFolder)) {
                    Directory.CreateDirectory(_dataFolder);
                }

                if (createDatabase && File.Exists(dataFile)) {
                    File.Delete(dataFile);
                }

                string localConnectionString = string.Format("Data Source={0}", dataFile);
                if (!File.Exists(dataFile)) {
                    CreateSqlCeDatabaseFile(localConnectionString);
                }

                persistence = persistence.ConnectionString(localConnectionString);
            }
            else {
                persistence = persistence.ConnectionString(_connectionString);
            }
            return persistence;
        }

        private void CreateSqlCeDatabaseFile(string connectionString) {
            // We want to execute this code using Reflection, to avoid having a binary
            // dependency on SqlCe assembly

            //engine engine = new SqlCeEngine();
            var sqlceEngineHandle = Activator.CreateInstance("System.Data.SqlServerCe", "System.Data.SqlServerCe.SqlCeEngine");
            var engine = sqlceEngineHandle.Unwrap();

            //engine.LocalConnectionString = connectionString;
            engine.GetType().GetProperty("LocalConnectionString").SetValue(engine, connectionString, null/*index*/);

            //engine.CreateDatabase();
            engine.GetType().GetMethod("CreateDatabase").Invoke(engine, null);

            //engine.Dispose();
            engine.GetType().GetMethod("Dispose").Invoke(engine, null);
        }

    }
}
