using System;
using System.Collections.Generic;
using Microsoft.ApplicationServer.Caching;
using NHibernate.Cache;
using Orchard.Azure.Services.Environment.Configuration;

namespace Orchard.Azure.Services.Caching.Database {

    public class AzureCacheProvider : ICacheProvider {

        private DataCache _dataCache;
        private DataCacheFactory _dataCacheFactory;

        public ICache BuildCache(string regionName, IDictionary<string, string> properties) {
            
            if (_dataCache == null) {
                throw new InvalidOperationException("Can't call this method when provider is in stopped state.");
            }
            
            TimeSpan? expiration = null;
            string expirationString;
            if (properties.TryGetValue(NHibernate.Cfg.Environment.CacheDefaultExpiration, out expirationString) || properties.TryGetValue("cache.default_expiration", out expirationString)) {
                expiration = TimeSpan.FromSeconds(Int32.Parse(expirationString));
            }

            return new AzureCacheClient(_dataCache, regionName, expiration);
        }

        public long NextTimestamp() {
            return Timestamper.Next();
        }

        public void Start(IDictionary<string, string> properties) {
            CacheClientConfiguration configuration;

            try {
                var tenantName = properties["cache.region_prefix"];

                bool enableCompression = false;
                string enableCompressionString;
                if (properties.TryGetValue("compression_enabled", out enableCompressionString))
                    enableCompression = Boolean.Parse(enableCompressionString);

                var pca = new DefaultPlatformConfigurationAccessor();
                configuration = CacheClientConfiguration.FromPlatformConfiguration(tenantName, Constants.DatabaseCacheSettingNamePrefix, pca);
                configuration.CompressionIsEnabled = enableCompression;
                configuration.Validate();
            }
            catch (Exception ex) {
                throw new Exception(String.Format("The {0} configuration settings are missing or invalid.", Constants.DatabaseCacheFeatureName), ex);
            }

            _dataCacheFactory = configuration.CreateCache();
            _dataCache = _dataCacheFactory.GetDefaultCache();
        }

        public void Stop() {
            _dataCache = null;
            _dataCacheFactory.Dispose();
        }
    }
}