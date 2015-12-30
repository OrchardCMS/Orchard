using System;
using FluentNHibernate.Cfg.Db;
using NHibernate.Cfg;

namespace Orchard.Data.Providers {
    public class SqlServerDataServicesProvider : AbstractDataServicesProvider {
        private readonly string _dataFolder;
        private readonly string _connectionString;

        public SqlServerDataServicesProvider(string dataFolder, string connectionString) {
            _dataFolder = dataFolder;
            _connectionString = connectionString;
        }

        public static string ProviderName {
            get { return "SqlServer"; }
        }

        public override IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase) {

            var persistence = MsSqlConfiguration.MsSql2008;
            if (string.IsNullOrEmpty(_connectionString)) {
                throw new ArgumentException("The connection string is empty");
            }

            persistence = persistence.ConnectionString(_connectionString);

            // use MsSql2012Dialect if on Azure or if specified in the connection string
            if (IsAzureSql()) {
                persistence = persistence.Dialect<NHibernate.Dialect.MsSql2012Dialect>();
            }

            return persistence;
        }

        protected override void AlterConfiguration(Configuration config) {
            config.SetProperty(NHibernate.Cfg.Environment.PrepareSql, Boolean.TrueString);
        }

        private bool IsAzureSql() {
            return _connectionString.ToLowerInvariant().Contains("database.windows.net");
        }
    }
}