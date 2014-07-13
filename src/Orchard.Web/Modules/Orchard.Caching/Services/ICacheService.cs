using System;

namespace Orchard.Caching.Services {
    public interface ICacheService : IDependency {

        object Get(string key);
        void Put(string key, object value);
        void Put(string key, object value, TimeSpan validFor);

        void Remove(string key);
        void Clear();
    }

    public static class CachingExtensions {

        public static T Get<T>(this ICacheService cacheService, string key) {
            return (T)cacheService.Get(key);
        }

        public static T Get<T>(this ICacheService cacheService, string key, Func<T> factory) {
            var result = cacheService.Get(key);
            if (result == null) {
                var computed = factory();
                cacheService.Put(key, computed);
                return computed;
            }

            // try to convert to T
            return (T)result;
        }

        public static T Get<T>(this ICacheService cacheService, string key, Func<T> factory, TimeSpan validFor) {
            var result = cacheService.Get(key);
            if (result == null) {
                var computed = factory();
                cacheService.Put(key, computed, validFor);
                return computed;
            }

            // try to convert to T
            return (T)result;
        }
    }
}