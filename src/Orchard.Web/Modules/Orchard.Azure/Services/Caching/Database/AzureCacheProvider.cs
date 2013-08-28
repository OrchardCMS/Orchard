using System.Collections.Generic;
using Microsoft.WindowsAzure.ServiceRuntime;
using NHibernate.Cache;
using System;
using Microsoft.ApplicationServer.Caching;
using NHibernate;

namespace Orchard.Azure.Services.Caching.Database {

    public class AzureCacheProvider : ICacheProvider {

        #region DataCache repository

        private static IDictionary<string, DataCache> _cacheDictionary = new Dictionary<string, DataCache>();

        private static DataCache GetCache(IInternalLogger logger, string cacheHostIdentifier, string cacheName, bool enableCompression) {
            string key = String.Format("{0}_{1}_{2}", cacheHostIdentifier, cacheName, enableCompression);
            if (!_cacheDictionary.ContainsKey(key)) {
                var dataCacheFactoryConfiguration = new DataCacheFactoryConfiguration() {
                    AutoDiscoverProperty = new DataCacheAutoDiscoverProperty(true, AzureCacheConfiguration.CacheHostIdentifier),
                    MaxConnectionsToServer = 32,
                    UseLegacyProtocol = false
                };

                var dataCacheFactory = new DataCacheFactory(dataCacheFactoryConfiguration);

                if (logger.IsDebugEnabled)
                    logger.DebugFormat("Creating DataCache with CacheHostIdentifier='{0}' and CacheName='{1}'.", cacheHostIdentifier, cacheName);

                DataCache newCache;
                if (!String.IsNullOrEmpty(cacheName))
                    newCache = dataCacheFactory.GetCache(cacheName);
                else
                    newCache = dataCacheFactory.GetDefaultCache();
                _cacheDictionary[key] = newCache;
            }
            else {
                if (logger.IsDebugEnabled)
                    logger.DebugFormat("Reusing existing DataCache with CacheHostIdentifier='{0}' and CacheName='{1}'.", cacheHostIdentifier, cacheName);
            }

            return _cacheDictionary[key];
        }

        #endregion

        public AzureCacheProvider() {
            _logger = LoggerProvider.LoggerFor(typeof(AzureCacheProvider));
        }

        private readonly IInternalLogger _logger;

        #region ICacheProvider Members

        public ICache BuildCache(string regionName, IDictionary<string, string> properties) {
            bool enableCompression = false;
            string enableCompressionString;
            if (properties.TryGetValue("compression_enabled", out enableCompressionString))
                enableCompression = Boolean.Parse(enableCompressionString);

            // Using static fields to communicate host identifier and cache name from AzureCacheConfiguration to
            // this class might cause problems in multi-tenancy scenarios when tenants have different settings
            // for these in shell settings. We should think of something more robust.
            var cache = GetCache(_logger, AzureCacheConfiguration.CacheHostIdentifier, AzureCacheConfiguration.CacheName, enableCompression);

            TimeSpan? expiration = null;
            string expirationString;
            if (properties.TryGetValue("expiration", out expirationString) || properties.TryGetValue(global::NHibernate.Cfg.Environment.CacheDefaultExpiration, out expirationString))
                expiration = TimeSpan.FromSeconds(Int32.Parse(expirationString));
 
            return new AzureCacheClient(cache, AzureCacheConfiguration.IsSharedCaching, regionName, expiration);
        }

        public long NextTimestamp() {
            return Timestamper.Next();
        }

        public void Start(IDictionary<string, string> properties) {
        }

        public void Stop() {
        }

        #endregion
    }
}