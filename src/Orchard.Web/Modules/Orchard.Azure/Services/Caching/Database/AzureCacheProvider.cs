using System.Collections.Generic;
using Microsoft.ApplicationServer.Caching;
using NHibernate.Cache;
using System;

namespace Orchard.Azure.Services.Caching.Database {

    public class AzureCacheProvider : ICacheProvider {

        private DataCache _dataCache;
        private bool _sharedCaching;

        public ICache BuildCache(string regionName, IDictionary<string, string> properties) {
            
            if (_dataCache == null) {
                throw new ApplicationException("DataCache should be available");
            }
            
            string enableCompressionString;
            properties.TryGetValue("compression_enabled", out enableCompressionString);

            
            TimeSpan? expiration = null;
            string expirationString;
            if (properties.TryGetValue(NHibernate.Cfg.Environment.CacheDefaultExpiration, out expirationString) || properties.TryGetValue("cache.default_expiration", out expirationString)) {
                expiration = TimeSpan.FromSeconds(Int32.Parse(expirationString));
            }

            return new AzureCacheClient(_dataCache, _sharedCaching, regionName, expiration);
        }

        public long NextTimestamp() {
            return Timestamper.Next();
        }

        public void Start(IDictionary<string, string> properties) {
            CacheClientConfiguration configuration;

            try {
                var tenant = properties["cache.region_prefix"];

                configuration = CacheClientConfiguration.FromPlatformConfiguration(tenant, Constants.DatabaseCacheSettingNamePrefix);
                configuration.Validate();
            }
            catch (Exception ex) {
                throw new Exception(String.Format("The {0} configuration settings are missing or invalid.", Constants.DatabaseCacheFeatureName), ex);
            }

            _dataCache = configuration.CreateCache();
            _sharedCaching = configuration.IsSharedCaching;
        }

        public void Stop() {
        }
    }
}