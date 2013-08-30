using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationServer.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;

namespace Orchard.Azure.Services.Caching.Output {

    [OrchardFeature(Constants.OutputCacheFeatureName)]
    [OrchardSuppressDependency("Orchard.OutputCache.Services.DefaultCacheStorageProvider")]
    public class AzureOutputCacheStorageProvider : Component, IOutputCacheStorageProvider {

        public AzureOutputCacheStorageProvider(ShellSettings shellSettings) {

            try {
                _cacheConfig = CacheClientConfiguration.FromPlatformConfiguration(shellSettings.Name, Constants.OutputCacheSettingNamePrefix);
                _cacheConfig.Validate();
            }
            catch (Exception ex) {
                throw new Exception(String.Format("The {0} configuration settings are missing or invalid.", Constants.OutputCacheFeatureName), ex);
            }

            _cache = _cacheConfig.CreateCache();

            if (!_cacheConfig.IsSharedCaching) {
                // If not using Windows Azure Shared Caching we can enable additional features by
                // storing all cache items in a region. This enables enumerating and counting all
                // items currently in the cache.
                _region = shellSettings.Name;
                _cache.CreateRegion(_region);
            }
        }

        private readonly CacheClientConfiguration _cacheConfig;
        private readonly DataCache _cache;
        private readonly string _region;

        public void Set(string key, CacheItem cacheItem) {
            Logger.Debug("Set() invoked with key='{0}' in region '{1}'.", key, _region);
            if (_cacheConfig.IsSharedCaching)
                _cache.Put(key, cacheItem);
            else
                _cache.Put(key, cacheItem, TimeSpan.FromSeconds(cacheItem.ValidFor), _region);
        }

        public void Remove(string key) {
            Logger.Debug("Remove() invoked with key='{0}' in region '{1}'.", key, _region);
            if (_cacheConfig.IsSharedCaching)
                _cache.Remove(key);
            else
                _cache.Remove(key, _region);
        }

        public void RemoveAll() {
            Logger.Debug("RemoveAll() invoked in region '{0}'.", _region);
            if (_cacheConfig.IsSharedCaching)
                _cache.Clear();
            else
                _cache.ClearRegion(_region);
        }

        public CacheItem GetCacheItem(string key) {
            Logger.Debug("GetCacheItem() invoked with key='{0}' in region '{1}'.", key, _region);
            if (_cacheConfig.IsSharedCaching)
                return _cache.Get(key) as CacheItem;
            else
                return _cache.Get(key, _region) as CacheItem;
        }

        public IEnumerable<CacheItem> GetCacheItems(int skip, int count) {
            Logger.Debug("GetCacheItems() invoked in region '{0}'.", _region);
            if (_cacheConfig.IsSharedCaching) {
                Logger.Debug("Enumeration not supported with Shared Caching; returning empty enumerable.");
                return Enumerable.Empty<CacheItem>(); // Enumeration not supported with Shared Caching.
            }
            
            return _cache.GetObjectsInRegion(_region).AsParallel()
                .Select(x => x.Value)
                .OfType<CacheItem>()
                .Skip(skip)
                .Take(count)
                .ToArray();
        }

        public int GetCacheItemsCount() {
            Logger.Debug("GetCacheItemsCount() invoked in region '{0}'.", _region);
            if (_cacheConfig.IsSharedCaching) {
                Logger.Debug("Enumeration not supported with Shared Caching; returning zero.");
                return 0; // Enumeration not supported with Shared Caching.
            }

            return _cache.GetObjectsInRegion(_region).AsParallel()
                .Select(x => x.Value)
                .OfType<CacheItem>()
                .Count();
        }
    }
}