using System;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using Orchard.Data.Providers;

namespace Orchard.Specs.Hosting {
    public class TraceEnabledDataServicesProviderFactory : IDataServicesProviderFactory {
        public IDataServicesProvider CreateProvider(DataServiceParameters sessionFactoryParameters) {
            return new TraceEnabledBuilder(sessionFactoryParameters.DataFolder, sessionFactoryParameters.ConnectionString);
        }

        class TraceEnabledBuilder : SQLiteDataServicesProvider {
            public TraceEnabledBuilder(string dataFolder, string connectionString) : base(dataFolder, connectionString) {
            }
            protected override IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase) {
                var config = (SQLiteConfiguration)base.GetPersistenceConfigurer(createDatabase);
                //config.ShowSql();
                return config;
            }
        }
    }
}