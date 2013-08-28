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

        private static DataCache GetCache(IInternalLogger logger, CacheClientConfiguration config) {
            string key = config.ToString();
            if (!_cacheDictionary.ContainsKey(key)) {
                if (logger.IsDebugEnabled)
                    logger.DebugFormat("Creating new DataCache with key '{0}'.", key);
                _cacheDictionary[key] = AzureCacheConfiguration.CacheClientConfiguration.CreateCache();
            }
            else {
                if (logger.IsDebugEnabled)
                    logger.DebugFormat("Reusing existing DataCache with key '{0}'.", key);
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
            // for these in platform configuration. We should think of something more robust.
            var cache = GetCache(_logger, AzureCacheConfiguration.CacheClientConfiguration);

            TimeSpan? expiration = null;
            string expirationString;
            if (properties.TryGetValue("expiration", out expirationString) || properties.TryGetValue(global::NHibernate.Cfg.Environment.CacheDefaultExpiration, out expirationString))
                expiration = TimeSpan.FromSeconds(Int32.Parse(expirationString));

            return new AzureCacheClient(cache, AzureCacheConfiguration.CacheClientConfiguration.IsSharedCaching, regionName, expiration);
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