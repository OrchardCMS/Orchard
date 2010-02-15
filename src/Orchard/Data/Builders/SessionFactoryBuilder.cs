using System;
using NHibernate;

namespace Orchard.Data.Builders {
    public class SessionFactoryBuilder : ISessionFactoryBuilder {
        public ISessionFactory BuildSessionFactory(SessionFactoryParameters sessionFactoryParameters) {
            AbstractBuilder builder;
            if (string.Equals(sessionFactoryParameters.Provider, "SQLite", StringComparison.InvariantCultureIgnoreCase)) {
                builder = new SQLiteBuilder(sessionFactoryParameters.DataFolder, sessionFactoryParameters.ConnectionString);
            }
            else {
                builder = new SqlServerBuilder(sessionFactoryParameters.DataFolder, sessionFactoryParameters.ConnectionString);
            }
            return builder.BuildSessionFactory(sessionFactoryParameters);
        }
    }
}
