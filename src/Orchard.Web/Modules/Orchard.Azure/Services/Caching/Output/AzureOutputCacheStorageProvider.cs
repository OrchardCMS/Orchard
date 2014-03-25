using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.ApplicationServer.Caching;
using Orchard.Azure.Services.Environment.Configuration;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;

namespace Orchard.Azure.Services.Caching.Output {

    [OrchardFeature(Constants.OutputCacheFeatureName)]
    [OrchardSuppressDependency("Orchard.OutputCache.Services.DefaultCacheStorageProvider")]
    public class AzureOutputCacheStorageProvider : Component, IOutputCacheStorageProvider {

        private readonly DataCache _cache;
        private readonly string _regionAlphaNumeric;

        public AzureOutputCacheStorageProvider(ShellSettings shellSettings, IAzureOutputCacheHolder cacheHolder, IPlatformConfigurationAccessor pca) {

            var region = shellSettings.Name;

            // Azure Cache supports only alphanumeric strings for regions, but Orchard supports some
            // non-alphanumeric characters in tenant names. Remove all non-alphanumering characters
            // from the region, and append the hash code of the original string to mitigate the risk
            // of two distinct original region strings yielding the same transformed region string.
            _regionAlphaNumeric = new String(Array.FindAll(region.ToCharArray(), Char.IsLetterOrDigit)) + region.GetHashCode().ToString(CultureInfo.InvariantCulture);

            _cache = cacheHolder.TryGetDataCache(() => {
                CacheClientConfiguration cacheConfig;

                try {
                    cacheConfig = CacheClientConfiguration.FromPlatformConfiguration(shellSettings.Name, Constants.OutputCacheSettingNamePrefix, pca);
                    cacheConfig.Validate();
                }
                catch (Exception ex) {
                    throw new Exception(String.Format("The {0} configuration settings are missing or invalid.", Constants.OutputCacheFeatureName), ex);
                }

                var cache = cacheConfig.CreateCache();
                cache.CreateRegion(_regionAlphaNumeric);

                return cache;
            });
        }

        public void Set(string key, CacheItem cacheItem) {
            if (cacheItem.ValidFor <= 0) {
                return;
            }

            Logger.Debug("Set() invoked with key='{0}' in region '{1}'.", key, _regionAlphaNumeric);
            _cache.Put(key, cacheItem, TimeSpan.FromSeconds(cacheItem.ValidFor), _regionAlphaNumeric);
        }

        public void Remove(string key) {
            Logger.Debug("Remove() invoked with key='{0}' in region '{1}'.", key, _regionAlphaNumeric);
            _cache.Remove(key, _regionAlphaNumeric);
        }

        public void RemoveAll() {
            Logger.Debug("RemoveAll() invoked in region '{0}'.", _regionAlphaNumeric);
            _cache.ClearRegion(_regionAlphaNumeric);
        }

        public CacheItem GetCacheItem(string key) {
            Logger.Debug("GetCacheItem() invoked with key='{0}' in region '{1}'.", key, _regionAlphaNumeric);
            return _cache.Get(key, _regionAlphaNumeric) as CacheItem;
        }

        public IEnumerable<CacheItem> GetCacheItems(int skip, int count) {
            Logger.Debug("GetCacheItems() invoked in region '{0}'.", _regionAlphaNumeric);
            return _cache.GetObjectsInRegion(_regionAlphaNumeric).AsParallel()
                .Select(x => x.Value)
                .OfType<CacheItem>()
                .Skip(skip)
                .Take(count)
                .ToArray();
        }

        public int GetCacheItemsCount() {
            Logger.Debug("GetCacheItemsCount() invoked in region '{0}'.", _regionAlphaNumeric);
            return _cache.GetObjectsInRegion(_regionAlphaNumeric).AsParallel()
                .Select(x => x.Value)
                .OfType<CacheItem>()
                .Count();
        }
    }
}