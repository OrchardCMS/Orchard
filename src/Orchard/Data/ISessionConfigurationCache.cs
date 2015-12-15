using System;
using NHibernate.Cfg;

namespace Orchard.Data {
    public interface ISessionConfigurationCache {
        Configuration GetConfiguration(Func<Configuration> builder);
    }
}