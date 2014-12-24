using System;

namespace Orchard.Caching.Services {
    public interface ICacheService : IDependency {
        object Get<T>(string key);

        void Put<T>(string key, T value);
        void Put<T>(string key, T value, TimeSpan validFor);

        void Remove(string key);
        void Clear();
    }

    public static class CachingExtensions {
        public static object Get<T>(this ICacheService cacheService, string key, Func<T> factory) {
            var result = cacheService.Get<T>(key);
            
            if (result == null) {
                var computed = factory();
                cacheService.Put(key, computed);
                return computed;
            }

            // try to convert to T
            return result;
        }

        public static object Get<T>(this ICacheService cacheService, string key, Func<T> factory, TimeSpan validFor) {
            var result = cacheService.Get<T>(key);
            
            if (result == null) {
                var computed = factory();
                cacheService.Put(key, computed, validFor);
                return computed;
            }

            // try to convert to T
            return result;
        }
    }
}