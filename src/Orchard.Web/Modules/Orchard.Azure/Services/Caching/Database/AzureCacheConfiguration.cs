using System;
using NHibernate.Cfg.Loquacious;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;

namespace Orchard.Azure.Services.Caching.Database {

    [OrchardFeature(Constants.DatabaseCacheFeatureName)]
    [OrchardSuppressDependency("Orchard.Data.DefaultDatabaseCacheConfiguration")]
    public class AzureCacheConfiguration : Component, IDatabaseCacheConfiguration {

        public AzureCacheConfiguration(ShellSettings shellSettings) {
            _shellSettings = shellSettings;
        }

        private readonly ShellSettings _shellSettings;

        public void Configure(ICacheConfigurationProperties cache) {
            cache.Provider<AzureCacheProvider>();
            cache.UseQueryCache = true;
            cache.RegionsPrefix = _shellSettings.Name;

            Logger.Information("Configured NHibernate cache provider '{0}' with regions prefix '{1}'.", typeof(AzureCacheProvider).Name, _shellSettings.Name);
        }
    }
}