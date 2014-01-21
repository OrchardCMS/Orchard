using System;
using Orchard.Environment.Configuration;

namespace Orchard.Caching.Services {
    /// <summary>
    /// Provides a per tenant <see cref="ICacheService"/> implementation.
    /// Default timeout is 20 minutes.
    /// </summary>
    public class DefaultCacheService : ICacheService {
        private readonly ICacheStorageProvider _cacheStorageProvider;
        private readonly string _prefix;

        public DefaultCacheService(
            ShellSettings shellSettings, 
            ICacheStorageProvider cacheStorageProvider) {
            _cacheStorageProvider = cacheStorageProvider;
            _prefix = shellSettings.Name;
        }

        public object Get(string key) {
            return _cacheStorageProvider.Get(BuildFullKey(key));
        }

        public void Put(string key, object value) {
            _cacheStorageProvider.Put(BuildFullKey(key), value);
        }

        public void Put(string key, object value, TimeSpan validFor) {
            _cacheStorageProvider.Put(BuildFullKey(key), value, validFor);
        }

        public void Remove(string key) {
            _cacheStorageProvider.Remove(BuildFullKey(key));
        }

        public void Clear() {
            _cacheStorageProvider.Clear();
        }

        private string BuildFullKey(string key) {
            return String.Concat(_prefix, "_", key);
        }
    }
}