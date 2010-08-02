using System;
using NHibernate.Cfg;

namespace Orchard.Data {
    public interface ISessionConfigurationCache : ISingletonDependency {
        Configuration GetConfiguration(Func<Configuration> builder);
    }
}