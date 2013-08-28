using System.Collections.Generic;
using Microsoft.WindowsAzure.ServiceRuntime;
using NHibernate.Cache;
using System;

namespace Orchard.Azure.Services.Caching.Database {

    public class AzureCacheProvider : ICacheProvider {

        #region ICacheProvider Members

        public ICache BuildCache(string regionName, IDictionary<string, string> properties) {
            bool enableCompression = false;
            string enableCompressionString;
            if (properties.TryGetValue("compression_enabled", out enableCompressionString))
                enableCompression = Boolean.Parse(enableCompressionString);

            TimeSpan? expiration = null;
            string expirationString;
            if (properties.TryGetValue("expiration", out expirationString) || properties.TryGetValue(global::NHibernate.Cfg.Environment.CacheDefaultExpiration, out expirationString))
                expiration = TimeSpan.FromSeconds(Int32.Parse(expirationString));

            return new AzureCacheClient(AzureCacheConfiguration.CacheHostIdentifier, AzureCacheConfiguration.CacheName, regionName, enableCompression, expiration);
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