using System;
using System.IO;
using FluentNHibernate.Cfg.Db;

namespace Orchard.Data.Providers {
    public class SqlCeDataServicesProvider : AbstractDataServicesProvider {
        private readonly string _fileName;
        private readonly string _dataFolder;
        private readonly string _connectionString;

        public SqlCeDataServicesProvider(string dataFolder, string connectionString) {
            _dataFolder = dataFolder;
            _connectionString = connectionString;
            _fileName = Path.Combine(_dataFolder, "Orchard.sdf");
        }

        public SqlCeDataServicesProvider(string fileName) {
            _dataFolder = Path.GetDirectoryName(fileName);
            _fileName = fileName;
        }

        public static string ProviderName {
            get { return "SqlCe"; }
        }

        public override IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase) {
            var persistence = MsSqlCeConfiguration.Standard;

            if (createDatabase) {
                File.Delete(_fileName);
            }

            string localConnectionString = string.Format("Data Source={0}", _fileName);
            if (!File.Exists(_fileName)) {
                CreateSqlCeDatabaseFile(localConnectionString);
            }

            persistence = persistence.ConnectionString(localConnectionString);
            return persistence;
        }

        private void CreateSqlCeDatabaseFile(string connectionString) {
            if (!string.IsNullOrEmpty(_dataFolder))
                Directory.CreateDirectory(_dataFolder);

            // We want to execute this code using Reflection, to avoid having a binary
            // dependency on SqlCe assembly

            //engine engine = new SqlCeEngine();
            //const string assemblyName = "System.Data.SqlServerCe, Version=4.0.0.1, Culture=neutral, PublicKeyToken=89845dcd8080cc91";
            const string assemblyName = "System.Data.SqlServerCe";
            const string typeName = "System.Data.SqlServerCe.SqlCeEngine";

            var sqlceEngineHandle = Activator.CreateInstance(assemblyName, typeName);
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
