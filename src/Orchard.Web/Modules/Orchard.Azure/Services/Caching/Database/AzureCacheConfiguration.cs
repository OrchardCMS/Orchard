using NHibernate.Cfg.Loquacious;
using Orchard;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using System.Linq;

namespace Orchard.Azure.Services.Caching.Database {

    [OrchardFeature(Constants.DatabaseCacheFeatureName)]
    [OrchardSuppressDependency("Orchard.Data.DefaultDatabaseCacheConfiguration")]
    public class AzureCacheConfiguration : Component, IDatabaseCacheConfiguration {

        public static string CacheHostIdentifier;
        public static string CacheName;

        public AzureCacheConfiguration(IShellSettingsManager shellSettingsManager, ShellSettings shellSettings)
            : base() {

            // Create default configuration to local role-based cache when feature is enabled.
            if (!shellSettings.Keys.Contains(Constants.DatabaseCacheHostIdentifierSettingName))
                shellSettings[Constants.DatabaseCacheHostIdentifierSettingName] = "Orchard.Azure.Web";
            if (!shellSettings.Keys.Contains(Constants.DatabaseCacheCacheNameSettingName))
                shellSettings[Constants.DatabaseCacheCacheNameSettingName] = "DatabaseCache";
            if (!shellSettings.Keys.Contains(Constants.DatabaseCacheIsSharedCachingSettingName))
                shellSettings[Constants.DatabaseCacheIsSharedCachingSettingName] = "false";

            shellSettingsManager.SaveSettings(shellSettings);

            CacheHostIdentifier = shellSettings[Constants.DatabaseCacheHostIdentifierSettingName];
            CacheName = shellSettings[Constants.DatabaseCacheCacheNameSettingName];

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