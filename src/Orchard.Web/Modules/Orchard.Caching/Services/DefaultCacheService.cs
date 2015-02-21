using System;
using Orchard.Environment.Configuration;

namespace Orchard.Caching.Services {
    /// <summary>
    /// Provides a per tenant <see cref="ICacheService"/> implementation.
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

        public object GetObject<T>(string key) {
            return _cacheStorageProvider.Get<T>(BuildFullKey(key));
        }

        public void Put<T>(string key, T value) {
            _cacheStorageProvider.Put(BuildFullKey(key), value);
        }

        public void Put<T>(string key, T value, TimeSpan validFor) {
            _cacheStorageProvider.Put(BuildFullKey(key), value, validFor);
        }

        public void Remove(string key) {
            _cacheStorageProvider.Remove(BuildFullKey(key));
        }

        public void Clear() {
            _cacheStorageProvider.Clear();
        }

        private string BuildFullKey(string key) {
            return String.Concat(_prefix, ":", key);
        }
    }
}