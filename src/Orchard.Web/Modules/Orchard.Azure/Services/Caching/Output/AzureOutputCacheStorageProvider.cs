using Microsoft.ApplicationServer.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Azure.Services.Caching.Output {

    [OrchardFeature(Constants.OutputCacheFeatureName)]
    [OrchardSuppressDependency("Orchard.OutputCache.Services.DefaultCacheStorageProvider")]
    public class AzureOutputCacheStorageProvider : Component, IOutputCacheStorageProvider {

        public AzureOutputCacheStorageProvider(IShellSettingsManager shellSettingsManager, ShellSettings shellSettings)
            : base() {

            // Create default configuration to local role-based cache when feature is enabled.
            if (!shellSettings.Keys.Contains(Constants.OutputCacheHostIdentifierSettingName))
                shellSettings[Constants.OutputCacheHostIdentifierSettingName] = "Orchard.Azure.Web";
            if (!shellSettings.Keys.Contains(Constants.OutputCacheCacheNameSettingName))
                shellSettings[Constants.OutputCacheCacheNameSettingName] = "OutputCache";
            if (!shellSettings.Keys.Contains(Constants.OutputCacheIsSharedCachingSettingName))
                shellSettings[Constants.OutputCacheIsSharedCachingSettingName] = "false";
            
            shellSettingsManager.SaveSettings(shellSettings);

            var cacheHostIdentifier = shellSettings[Constants.OutputCacheHostIdentifierSettingName];
            var cacheName = shellSettings[Constants.OutputCacheCacheNameSettingName];

            var dataCacheFactoryConfiguration = new DataCacheFactoryConfiguration() {
                AutoDiscoverProperty = new DataCacheAutoDiscoverProperty(true, cacheHostIdentifier),
                MaxConnectionsToServer = 32,
                UseLegacyProtocol = false
            };

            var dataCacheFactory = new DataCacheFactory(dataCacheFactoryConfiguration);
            
            if (!String.IsNullOrEmpty(cacheName))
                _cache = dataCacheFactory.GetCache(cacheName);
            else
                _cache = dataCacheFactory.GetDefaultCache();

            _usingSharedCaching = Boolean.Parse(shellSettings[Constants.OutputCacheIsSharedCachingSettingName]);
 
            if (!_usingSharedCaching)
            {
                // If not using Windows Azure Shared Caching we can enable additional features by
                // storing all cache items in a region. This enables enumerating and counting all
                // items currently in the cache.
                _region = shellSettings.Name;
                _cache.CreateRegion(_region);           
            }
        }

        private readonly DataCache _cache;
        private readonly bool _usingSharedCaching; // True if using Windows Azure Shared Caching rather than role-based caching.
        private readonly string _region;

        public void Set(string key, CacheItem cacheItem) {
            if (_usingSharedCaching)
                _cache.Put(key, cacheItem);
            else
                _cache.Put(key, cacheItem, _region);
        }

        public void Remove(string key) {
            if (_usingSharedCaching)
                _cache.Remove(key);
            else
                _cache.Remove(key, _region);
        }

        public void RemoveAll() {
            if (_usingSharedCaching)
                _cache.Clear();
            else
                _cache.ClearRegion(_region);
        }

        public CacheItem GetCacheItem(string key) {
            if (_usingSharedCaching)
                return _cache.Get(key) as CacheItem;
            else
                return _cache.Get(key, _region) as CacheItem;
        }

        public IEnumerable<CacheItem> GetCacheItems(int skip, int count) {
            if (_usingSharedCaching)
                throw new NotSupportedException("The GetCacheItems() method is not supported with Windows Azure Shared Caching.");
            
            return _cache.GetObjectsInRegion(_region).AsParallel()
                .Select(x => x.Value)
                .OfType<CacheItem>()
                .Skip(skip)
                .Take(count)
                .ToArray();
        }

        public int GetCacheItemsCount() {
            if (_usingSharedCaching)
                throw new NotSupportedException("The GetCacheItemsCount() method is not supported with Windows Azure Shared Caching.");

            return _cache.GetObjectsInRegion(_region).AsParallel()
                .Select(x => x.Value)
                .OfType<CacheItem>()
                .Count();
        }
    }
}