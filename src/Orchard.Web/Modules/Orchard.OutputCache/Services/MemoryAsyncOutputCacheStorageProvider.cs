using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Orchard.OutputCache.Models;
using Orchard.Environment.Configuration;
using System.Threading.Tasks;
using Orchard.Environment.Extensions;

namespace Orchard.OutputCache.Services {
    [OrchardFeature("Orchard.OutputCache2")]
    public class MemoryAsyncOutputCacheStorageProvider : IAsyncOutputCacheStorageProvider {
        private readonly string _tenantName;
        private readonly WorkContext _workContext;

        public MemoryAsyncOutputCacheStorageProvider(IWorkContextAccessor workContextAccessor, ShellSettings shellSettings) {
            _workContext = workContextAccessor.GetContext();
            _tenantName = shellSettings.Name;
        }

        public async Task SetAsync(string key, CacheItem cacheItem) {
            _workContext.HttpContext.Cache.Add(
                key,
                cacheItem,
                null,
                cacheItem.StoredUntilUtc,
                System.Web.Caching.Cache.NoSlidingExpiration,
                System.Web.Caching.CacheItemPriority.Normal,
                null);
        }

        public async Task<CacheItem> GetAsync(string key) {
            return _workContext.HttpContext.Cache.Get(key) as CacheItem;
        }

        public async Task RemoveAsync(string key) {
            _workContext.HttpContext.Cache.Remove(key);
        }

        public async Task RemoveAllAsync() {
            var items = (await GetAllAsync(0, 100)).ToList();
            while (items.Any()) {
                foreach (var item in items) {
                    await RemoveAsync(item.CacheKey);
                }
                items = (await GetAllAsync(0, 100)).ToList();
            }
        }


        public async Task<IEnumerable<CacheItem>> GetAllAsync(int skip, int count) {
            // the ASP.NET cache can also contain other types of items
            return _workContext.HttpContext.Cache.AsParallel()
                        .Cast<DictionaryEntry>()
                        .Select(x => x.Value)
                        .OfType<CacheItem>()
                        .Where(x => x.Tenant.Equals(_tenantName, StringComparison.OrdinalIgnoreCase))
                        .Skip(skip)
                        .Take(count);
        }

        public async Task<int> CountAsync() {
            return _workContext.HttpContext.Cache.AsParallel()
                        .Cast<DictionaryEntry>()
                        .Select(x => x.Value)
                        .OfType<CacheItem>()
                        .Count(x => x.Tenant.Equals(_tenantName, StringComparison.OrdinalIgnoreCase));
        }
    }
}