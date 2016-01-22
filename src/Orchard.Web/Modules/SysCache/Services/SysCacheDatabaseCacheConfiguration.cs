using NHibernate.Cfg.Loquacious;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;

namespace SysCache.Services {
    [OrchardSuppressDependency("Orchard.Data.DefaultDatabaseCacheConfiguration")]
    public class SysCacheDatabaseCacheConfiguration : IDatabaseCacheConfiguration {
        private readonly ShellSettings _shellSettings;

        public SysCacheDatabaseCacheConfiguration(ShellSettings shellSettings) {
            _shellSettings = shellSettings;
        }

        public void Configure(ICacheConfigurationProperties cache) {
            cache.Provider<NHibernate.Caches.SysCache2.SysCacheProvider>();
            cache.UseQueryCache = true;
            cache.RegionsPrefix = _shellSettings.Name;
        }
    }
}