using System;
using FluentNHibernate.Cfg.Db;
using NHibernate.Cfg;
using NHibernate.SqlAzure;

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

            // when using Sql Server Azure, use a specific driver, c.f. https://orchard.codeplex.com/workitem/19315
            if (IsAzureSql()) {
                persistence = persistence.Driver<SqlAzureClientDriver>();
            }

            return persistence;
        }

        protected override void AlterConfiguration(Configuration config) {
            config.SetProperty(NHibernate.Cfg.Environment.PrepareSql, Boolean.TrueString);

            if (IsAzureSql()) {
                config.SetProperty(NHibernate.Cfg.Environment.TransactionStrategy, typeof(ReliableAdoNetWithDistributedTransactionFactory).AssemblyQualifiedName);
            }
        }

        private bool IsAzureSql() {
            return _connectionString.ToLowerInvariant().Contains("database.windows.net");
        }
    }
}