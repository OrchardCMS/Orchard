using System;
using NHibernate;

namespace Orchard.Data.Builders {
    public class SessionFactoryBuilder : ISessionFactoryBuilder {
        public ISessionFactory BuildSessionFactory(SessionFactoryParameters parameters) {
            AbstractBuilder builder;
            if (string.Equals(parameters.Provider, "SQLite", StringComparison.InvariantCultureIgnoreCase)) {
                builder = new SQLiteBuilder(parameters.DataFolder, parameters.ConnectionString);
            }
            else {
                builder = new SqlServerBuilder(parameters.DataFolder, parameters.ConnectionString);
            }
            return builder.BuildSessionFactory(parameters);
        }
    }
}
