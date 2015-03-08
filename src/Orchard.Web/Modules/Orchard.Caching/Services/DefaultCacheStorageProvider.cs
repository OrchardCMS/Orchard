using System;
using System.Globalization;
using System.Runtime.Caching;
using Orchard.Services;

namespace Orchard.Caching.Services {
    // The technique of signaling tenant-specific cache entries to be invalidated comes from: http://stackoverflow.com/a/22388943/220230
    // Singleton so signals can be stored for the shell lifetime.
    public class DefaultCacheStorageProvider : ICacheStorageProvider, ISingletonDependency {
        private event EventHandler Signaled;

        private readonly IClock _clock;
        // MemoryCache is optimal with one instance, see: http://stackoverflow.com/questions/8463962/using-multiple-instances-of-memorycache/13425322#13425322
        private readonly MemoryCache _cache = MemoryCache.Default;

        public DefaultCacheStorageProvider(IClock clock) {
            _clock = clock;
        }

        public void Put<T>(string key, T value) {
            // Keys are already prefixed by DefaultCacheService so no need to do it here again.
            _cache.Set(key, value, GetCacheItemPolicy(ObjectCache.InfiniteAbsoluteExpiration));
        }

        public void Put<T>(string key, T value, TimeSpan validFor) {
            _cache.Set(key, value, GetCacheItemPolicy(new DateTimeOffset(_clock.UtcNow.Add(validFor))));
        }

        public void Remove(string key) {
            _cache.Remove(key);
        }

        public void Clear() {
            if (Signaled != null) {
                Signaled(null, EventArgs.Empty);
            }
        }

        public object Get<T>(string key) {
            return _cache.Get(key);
        }

        private CacheItemPolicy GetCacheItemPolicy(DateTimeOffset absoluteExpiration) {
            var cacheItemPolicy = new CacheItemPolicy {
                AbsoluteExpiration = absoluteExpiration, 
                SlidingExpiration = ObjectCache.NoSlidingExpiration
            };

            cacheItemPolicy.ChangeMonitors.Add(new TenantCacheClearMonitor(this));

            return cacheItemPolicy;
        }

        public class TenantCacheClearMonitor : ChangeMonitor {
            private readonly DefaultCacheStorageProvider _storageProvider;

            private readonly string _uniqueId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            public override string UniqueId {
                get { return _uniqueId; }
            }

            public TenantCacheClearMonitor(DefaultCacheStorageProvider storageProvider) {
                _storageProvider = storageProvider;
                _storageProvider.Signaled += OnSignalRaised;
                InitializationComplete();
            }

            protected override void Dispose(bool disposing) {
                Dispose();
                _storageProvider.Signaled -= OnSignalRaised;
            }

            private void OnSignalRaised(object sender, EventArgs e) {
                // Cache objects are obligated to remove entry upon change notification.
                OnChanged(null);
            }
        }
    }
}