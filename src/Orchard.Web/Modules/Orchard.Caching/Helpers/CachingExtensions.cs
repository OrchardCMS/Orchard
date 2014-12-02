using System;
using Orchard.Caching.Services;

namespace Orchard.Caching.Helpers {
    public static class CachingExtensions {
        public static T Get<T>(this ICacheStorageProvider provider, string key) {
            return (T)provider.Get(key);
        }

        public static T Get<T>(this ICacheService cacheService, string key) {
            return Get<T>(cacheService, key, null);
        }

        public static T Get<T>(this ICacheService cacheService, string key, TimeSpan validFor) {
            return Get<T>(cacheService, key, null, validFor);
        }

        public static T Get<T>(this ICacheService cacheService, string key, Func<T> factory) {
            return Get(cacheService, key, factory, TimeSpan.MinValue);
        }

        public static T Get<T>(this ICacheService cacheService, string key, Func<T> factory, TimeSpan validFor) {
            var result = cacheService.Get(key);

            if (result == null && factory != null) {
                var computed = factory();

                if (validFor == TimeSpan.MinValue)
                    cacheService.Put(key, computed);
                else
                    cacheService.Put(key, computed, validFor);
                return computed;
            }

            return (T)result;
        }
    }
}