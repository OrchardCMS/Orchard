using System;
using FluentNHibernate.Cfg.Db;

namespace Orchard.Data.Providers
{
    public class MySqklDataServicesProvider : AbstractDataServicesProvider
    {
        private readonly string _dataFolder;
        private readonly string _connectionString;

        public MySqklDataServicesProvider(string dataFolder, string connectionString)
        {
            _dataFolder = dataFolder;
            _connectionString = connectionString;
        }

        public static string ProviderName
        {
            get { return "MySql"; }
        }

        public override IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase)
        {
            var persistence = MySQLConfiguration.Standard;
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentException("The connection string is empty");
            }
            persistence = persistence.ConnectionString(_connectionString);
            return persistence;
        }
    }
}