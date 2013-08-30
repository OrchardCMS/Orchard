using System.Collections.Generic;
using NHibernate.Cache;
using System;

namespace Orchard.Azure.Services.Caching.Database {

    public class AzureCacheProvider : ICacheProvider {

        public ICache BuildCache(string regionName, IDictionary<string, string> properties) {
            CacheClientConfiguration configuration;
            
            try {
                configuration = CacheClientConfiguration.FromPlatformConfiguration(regionName, Constants.DatabaseCacheSettingNamePrefix);
                configuration.Validate();
            }
            catch (Exception ex) {
                throw new Exception(String.Format("The {0} configuration settings are missing or invalid.", Constants.DatabaseCacheFeatureName), ex);
            }

            string enableCompressionString;
            properties.TryGetValue("compression_enabled", out enableCompressionString);

            var cache = configuration.CreateCache();

            TimeSpan? expiration = null;
            string expirationString;
            if (properties.TryGetValue("expiration", out expirationString) || properties.TryGetValue(NHibernate.Cfg.Environment.CacheDefaultExpiration, out expirationString)) {
                expiration = TimeSpan.FromSeconds(Int32.Parse(expirationString));
            }

            return new AzureCacheClient(cache, configuration.IsSharedCaching, regionName, expiration);
        }

        public long NextTimestamp() {
            return Timestamper.Next();
        }

        public void Start(IDictionary<string, string> properties) {
        }

        public void Stop() {
        }
    }
}