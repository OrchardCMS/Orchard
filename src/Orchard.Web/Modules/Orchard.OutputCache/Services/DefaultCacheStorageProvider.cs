using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Orchard.OutputCache.Models;
using Orchard.Environment.Configuration;

namespace Orchard.OutputCache.Services {
    public class DefaultCacheStorageProvider : IOutputCacheStorageProvider {
        private readonly string _tenantName;
        private readonly WorkContext _workContext;

        public DefaultCacheStorageProvider(IWorkContextAccessor workContextAccessor, ShellSettings shellSettings) {
            _workContext = workContextAccessor.GetContext();
            _tenantName = shellSettings.Name;
        }

        public void Set(string key, CacheItem cacheItem) {
            _workContext.HttpContext.Cache.Remove(key);
            _workContext.HttpContext.Cache.Add(
                key,
                cacheItem,
                null,
                cacheItem.StoredUntilUtc,
                System.Web.Caching.Cache.NoSlidingExpiration,
                System.Web.Caching.CacheItemPriority.Normal,
                null);
        }

        public void Remove(string key) {
            if (_workContext.HttpContext != null)
                _workContext.HttpContext.Cache.Remove(key);
        }

        public void RemoveAll() {
            var items = GetCacheItems(0, 100).ToList();
            while (items.Any()) {
                foreach (var item in items) {
                    Remove(item.CacheKey);
                }
                items = GetCacheItems(0, 100).ToList();
            }
        }

        public CacheItem GetCacheItem(string key) {
            return _workContext.HttpContext.Cache.Get(key) as CacheItem;
        }

        public IEnumerable<CacheItem> GetCacheItems(int skip, int count) {
            // the ASP.NET cache can also contain other types of items
            return _workContext.HttpContext.Cache.AsParallel()
                        .Cast<DictionaryEntry>()
                        .Select(x => x.Value)
                        .OfType<CacheItem>()
                        .Where(x => x.Tenant.Equals(_tenantName, StringComparison.OrdinalIgnoreCase))
                        .Skip(skip)
                        .Take(count);
        }

        public int GetCacheItemsCount() {
            return _workContext.HttpContext.Cache.AsParallel()
                        .Cast<DictionaryEntry>()
                        .Select(x => x.Value)
                        .OfType<CacheItem>()
                        .Count(x => x.Tenant.Equals(_tenantName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
