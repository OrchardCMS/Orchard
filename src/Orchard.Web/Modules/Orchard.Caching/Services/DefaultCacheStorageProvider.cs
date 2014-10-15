using System;
using System.Collections;
using System.Linq;
using System.Web;

namespace Orchard.Caching.Services {
    public class DefaultCacheStorageProvider : ICacheStorageProvider {

        public void Put<T>(string key, T value) {
            HttpRuntime.Cache.Insert(
                key,
                value,
                null,
                System.Web.Caching.Cache.NoAbsoluteExpiration,
                System.Web.Caching.Cache.NoSlidingExpiration,
                System.Web.Caching.CacheItemPriority.Normal,
                null);
        }

        public void Put<T>(string key, T value, TimeSpan validFor) {
            HttpRuntime.Cache.Insert(
                key,
                value,
                null,
                System.Web.Caching.Cache.NoAbsoluteExpiration,
                validFor,
                System.Web.Caching.CacheItemPriority.Normal,
                null);
        }

        public void Remove(string key) {
            HttpRuntime.Cache.Remove(key);
        }

        public void Clear() {
            var all = HttpRuntime.Cache
                .AsParallel()
                .Cast<DictionaryEntry>()
                .Select(x => x.Key.ToString())
                .ToList();

            foreach (var key in all) {
                Remove(key);
            }
        }

        public T Get<T>(string key) {
            var value = HttpRuntime.Cache.Get(key);
            if (value is T) {
                return (T)value;
            }

            return default(T);
        }
    }
}