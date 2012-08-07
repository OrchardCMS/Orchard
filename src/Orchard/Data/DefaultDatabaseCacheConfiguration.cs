using NHibernate.Cfg.Loquacious;

namespace Orchard.Data {
    public class DefaultDatabaseCacheConfiguration : IDatabaseCacheConfiguration {
        public void Configure(ICacheConfigurationProperties cache) {
            // do nothing, no cache
        }
    }
}