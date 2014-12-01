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

            if (!result.HasValue && factory != null) {
                var computed = factory();
                putter(computed);
                return computed;
            }

            return result;
        }
    }

    /// <summary>
    /// Wrapper class for a cache entry.
    /// </summary>
    /// <typeparam name="T">Type of the wrapped cache value.</typeparam>
    public class Cached<T> {
        /// <summary>
        /// Cache entry value.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// Checks if an entry is not empty.
        /// </summary>
        public bool HasValue { get; private set; }

        /// <summary>
        /// Expiration date of this entry.
        /// </summary>
        public DateTimeOffset Expires { get; set; }

        protected Cached(object value) {
            if (value == null) {
                HasValue = false;
                Value = default(T);
            }
            else {
                HasValue = true;
                Value = (T)value;
            }

            Expires = DateTimeOffset.MaxValue;
        }

        public static implicit operator Cached<T>(T value) {
            return new Cached<T>(value);
        }

        public static explicit operator T(Cached<T> value) {
            return value.Value;
        }

        public static bool operator ==(Cached<T> a, T b) {
            if (a == null) {
                return false;
            }

            if (a.HasValue) {
                return a.Value.Equals(b);
            }

            if (typeof(T).IsValueType) {
                return b.Equals(default(T));
            }

            return b == null;
        }

        public static bool operator ==(T a, Cached<T> b) {
            return b == a;
        }

        public static bool operator !=(T a, Cached<T> b) {
            return !(a == b);
        }

        public static bool operator !=(Cached<T> a, T b) {
            return !(a == b);
        }

        public override bool Equals(object other)
        {
            if (!HasValue) return other == null;
            if (other == null) return false;
            return Value.Equals(other);
        }

        public override int GetHashCode()
        {
            return HasValue ? Value.GetHashCode() : 0;
        }
    }
}