using FluentNHibernate.Cfg.Db;
using NHibernate;
using Orchard.Data.Builders;

namespace Orchard.Specs.Hosting {
    public class TraceEnabledSessionFactoryBuilder : ISessionFactoryBuilder {
        public TraceEnabledSessionFactoryBuilder() {
            
        }
        public ISessionFactory BuildSessionFactory(SessionFactoryParameters parameters) {
            var builder = new TraceEnabledBuilder(parameters.DataFolder, parameters.ConnectionString);
            return builder.BuildSessionFactory(parameters);
        }

        class TraceEnabledBuilder : SQLiteBuilder {
            public TraceEnabledBuilder(string dataFolder, string connectionString) : base(dataFolder, connectionString) {
            }
            protected override IPersistenceConfigurer GetPersistenceConfigurer(bool createDatabase) {
                var config = (SQLiteConfiguration)base.GetPersistenceConfigurer(createDatabase);
                return config.ShowSql();
            }
        }
    }
}