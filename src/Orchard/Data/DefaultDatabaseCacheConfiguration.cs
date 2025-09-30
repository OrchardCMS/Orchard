using NHibernate.Cfg.Loquacious;

namespace Orchard.Data {
    public class DefaultDatabaseCacheConfiguration : IDatabaseCacheConfiguration {
        public void Configure(CacheConfigurationProperties cache) {
            cache.UseQueryCache = false;
        }
    }
}