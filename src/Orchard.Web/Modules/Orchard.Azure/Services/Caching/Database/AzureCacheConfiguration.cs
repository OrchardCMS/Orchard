using NHibernate.Cfg.Loquacious;
using Orchard;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;

namespace Orchard.Azure.Services.Caching.Database {

    [OrchardFeature("Orchard.Azure.DatabaseCache")]
    [OrchardSuppressDependency("Orchard.Data.DefaultDatabaseCacheConfiguration")]
    public class AzureCacheConfiguration : Component, IDatabaseCacheConfiguration {

        public static string CacheHostIdentifier;
        public static string CacheName;

        public AzureCacheConfiguration(ShellSettings shellSettings)
            : base() {
            _shellSettings = shellSettings;

            CacheHostIdentifier = shellSettings["Azure.DatabaseCache.HostIdentifier"];
            CacheName = shellSettings["Azure.DatabaseCache.CacheName"];
        }

        private readonly ShellSettings _shellSettings;

        public void Configure(ICacheConfigurationProperties cache) {
            cache.Provider<AzureCacheProvider>();
            cache.UseQueryCache = true;
            cache.RegionsPrefix = _shellSettings.Name;
            //cache.RegionsPrefix = "Orchard";

            Logger.Information("Configured NHibernate cache provider '{0}' with regions prefix '{1}'.", typeof(AzureCacheProvider).Name, _shellSettings.Name);
        }
    }
}