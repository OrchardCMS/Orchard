using System.Collections.Generic;
using Microsoft.WindowsAzure.ServiceRuntime;
using NHibernate.Cache;

namespace Orchard.Azure.Services.Caching.Database {

    public class AzureCacheProvider : ICacheProvider {

        #region ICacheProvider Members

        public ICache BuildCache(string regionName, IDictionary<string, string> properties) {
            return new AzureCacheClient(AzureCacheConfiguration.CacheHostIdentifier, AzureCacheConfiguration.CacheName, regionName);
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