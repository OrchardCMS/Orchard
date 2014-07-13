using System;
using System.Collections;
using System.Linq;
using System.Web;

namespace Orchard.Caching.Services {
    public class DefaultCacheStorageProvider : ICacheStorageProvider {

        public void Put(string key, object value) {
            HttpRuntime.Cache.Insert(
                key,
                value,
                null,
                System.Web.Caching.Cache.NoAbsoluteExpiration,
                System.Web.Caching.Cache.NoSlidingExpiration,
                System.Web.Caching.CacheItemPriority.Normal,
                null);
        }

        public void Put(string key, object value, TimeSpan validFor) {
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

        public object Get(string key) {
            return HttpRuntime.Cache.Get(key);
        }
    }
}