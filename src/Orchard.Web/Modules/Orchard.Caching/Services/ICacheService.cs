using System;

namespace Orchard.Caching.Services {
    public interface ICacheService : IDependency {
        Cached<T> Get<T>(string key);

        void Put<T>(string key, T value);
        void Put<T>(string key, T value, TimeSpan validFor);

        void Remove(string key);
        void Clear();
    }

    public static class CachingExtensions {
        public static Cached<T> Get<T>(this ICacheService cacheService, string key, Func<T> factory) {
            return cacheService.GetOrPut(key, factory, value => cacheService.Put(key, value));
        }

        public static Cached<T> Get<T>(this ICacheService cacheService, string key, Func<T> factory, TimeSpan validFor) {
            return cacheService.GetOrPut(key, factory, value => cacheService.Put(key, value, validFor));
        }

        private static Cached<T> GetOrPut<T>(this ICacheService cacheService, string key, Func<T> factory, Action<T> putter) {
            var result = cacheService.Get<T>(key);

            if (result == null && factory != null) {
                var computed = factory();
                putter(computed);
                return new Cached<T>(computed);
            }

            return result;
        }
    }

    public class Cached<T> {
        public T Value { get; private set; }
        public bool HasValue { get; private set; }

        public Cached(object value) {
            if (value == null) {
                HasValue = false;
                Value = default(T);
            }
            else {
                HasValue = true;
                Value = (T)value;
            }
        }

        public static implicit operator Cached<T>(T value)
        {
            return new Cached<T>(value);
        }

        public static implicit operator T(Cached<T> value)
        {
            return value.HasValue ? value.Value : default(T);
        }

        public static bool operator ==(Cached<T> a, T b) {
            if (a.HasValue) {
                return a.Value.Equals(b);
            }

            if (typeof(T).IsValueType) {
                return b.Equals(default(T));
            }

            return b == null;
        }

        public static bool operator !=(Cached<T> a, T b) {
            return !(a == b);
        }
    }
}