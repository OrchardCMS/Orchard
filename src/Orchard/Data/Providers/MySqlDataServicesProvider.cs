using System;
using FluentNHibernate.Cfg.Db;
using NHibernate.Cfg;

namespace Orchard.Data.Providers {
    public class MySqlDataServicesProvider : AbstractDataServicesProvider {
        private readonly string _dataFolder;
        private readonly string _connectionString;

        public MySqlDataServicesProvider(string dataFolder, string connectionString) {
            _dataFolder = dataFolder;
            _connectionString = connectionString;
        }

        public static string ProviderName {
            get { return "MySql"; }
        }

        public override IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase) {
            var persistence = MySQLConfiguration.Standard;
            if (string.IsNullOrEmpty(_connectionString)) {
                throw new ArgumentException("The connection string is empty");
            }
            persistence = persistence.ConnectionString(_connectionString);
            return persistence;
        }

        protected override void AlterConfiguration(Configuration config) {
            config.SetProperty(NHibernate.Cfg.Environment.PrepareSql, Boolean.TrueString);
        }
    }
}