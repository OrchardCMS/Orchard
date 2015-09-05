using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Cfg;
using NHibernate.Dialect;

namespace Orchard.Data.Migration.Interpreters
{
    public interface IDialectService : IDependency {
        Dialect GetDialect(Configuration config);
    }

    public class DialectService : IDialectService
    {
        public Dialect GetDialect(Configuration config) {
            return Dialect.GetDialect(config.Properties);
        }
    }
}
