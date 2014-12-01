using System;
using System.Globalization;
using System.Runtime.Caching;
using Orchard.Environment.Configuration;
using Orchard.Services;

namespace Orchard.Caching.Services {
    // The technique of signaling tenant-specific cache entries to be invalidated comes from: http://stackoverflow.com/a/22388943/220230
    // Singleton so signals can be stored for the shell lifetime.
    public class DefaultCacheStorageProvider : ICacheStorageProvider, ISingletonDependency {
        private event EventHandler Signaled;

        private readonly ShellSettings _shellSettings;
        private readonly IClock _clock;
        // MemoryCache is optimal with one instance, see: http://stackoverflow.com/questions/8463962/using-multiple-instances-of-memorycache/13425322#13425322
        private readonly MemoryCache _cache = MemoryCache.Default;

        public DefaultCacheStorageProvider(ShellSettings shellSettings, IClock clock) {
            _shellSettings = shellSettings;
            _clock = clock;
        }

        public void Put<T>(string key, T value) {
            // Keys are already prefixed by DefaultCacheService so no need to do it here again.
            _cache.Set(
                key, 
                BuildCacheItem(value, ObjectCache.InfiniteAbsoluteExpiration), 
                GetCacheItemPolicy(ObjectCache.InfiniteAbsoluteExpiration));
        }

        public void Put<T>(string key, T value, TimeSpan validFor) {
            var expiration = new DateTimeOffset(_clock.UtcNow).ToOffset(validFor);
            _cache.Set(
                key, 
                BuildCacheItem(value, expiration), 
                GetCacheItemPolicy(expiration));
        }

        public void Remove(string key) {
            _cache.Remove(key);
        }

        public void Clear() {
            if (Signaled != null) {
                Signaled(null, EventArgs.Empty);
            }
        }

        public Cached<T> Get<T>(string key) {
            var value = _cache.Get(key);
            // if the provided expression is non-null, and the provided object can 
            // be cast to the provided type without causing an exception to be thrown
            if (value is Cached<T>) {
                return (Cached<T>)value;
            }

            if (value is T) {
                return (T)value;
            }

            value = null;
            return (T)value;
        }

        private Cached<T> BuildCacheItem<T>(T item, DateTimeOffset expiration) {
            Cached<T> cached = item;
            cached.Expires = expiration;

            return cached;
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
                base.InitializationComplete();
            }

            protected override void Dispose(bool disposing) {
                base.Dispose();
                _storageProvider.Signaled -= OnSignalRaised;
            }

            private void OnSignalRaised(object sender, EventArgs e) {
                // Cache objects are obligated to remove entry upon change notification.
                base.OnChanged(null);
            }
        }
    }
}