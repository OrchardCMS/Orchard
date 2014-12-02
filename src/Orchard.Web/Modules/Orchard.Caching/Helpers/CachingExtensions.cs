using System;
using Orchard.Caching.Services;

namespace Orchard.Caching.Helpers {
    public static class CachingExtensions {
        public static T GetValue<T>(this ICacheStorageProvider provider, string key) {
            return (T)provider.Get<T>(key);
        }

        public static T GetValue<T>(this ICacheService cacheService, string key) {
            return GetValue<T>(cacheService, key, null);
        }

        public static T GetValue<T>(this ICacheService cacheService, string key, TimeSpan validFor) {
            return GetValue<T>(cacheService, key, null, validFor);
        }

        public static T GetValue<T>(this ICacheService cacheService, string key, Func<T> factory) {
            return GetValue(cacheService, key, factory, TimeSpan.MinValue);
        }

        public static T GetValue<T>(this ICacheService cacheService, string key, Func<T> factory, TimeSpan validFor) {
            var result = cacheService.Get<T>(key);

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