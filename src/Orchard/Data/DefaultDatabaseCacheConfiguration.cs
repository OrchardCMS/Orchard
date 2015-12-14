using NHibernate.Cfg.Loquacious;

namespace Orchard.Data {
    public class DefaultDatabaseCacheConfiguration : IDatabaseCacheConfiguration {
        public void Configure(ICacheConfigurationProperties cache) {
            cache.UseQueryCache = false;
        }
    }
}