using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.ApplicationServer.Caching;
using Orchard.Azure.Services.Environment.Configuration;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;

namespace Orchard.Azure.Services.Caching.Output {

    [OrchardFeature(Constants.OutputCacheFeatureName)]
    [OrchardSuppressDependency("Orchard.OutputCache.Services.DefaultCacheStorageProvider")]
    public class AzureOutputCacheStorageProvider : Component, IOutputCacheStorageProvider {
        public const string DataCacheKey = "DataCache";
        public const string ClientConfigurationKey = "CacheClientConfiguration";
        public const int Retries = 2;

        private readonly string _regionAlphaNumeric;
        private readonly ICacheManager _cacheManager;
        private readonly ShellSettings _shellSettings;
        private readonly IPlatformConfigurationAccessor _pca;
        private readonly ISignals _signals;

        public AzureOutputCacheStorageProvider(
            ShellSettings shellSettings, 
            IPlatformConfigurationAccessor pca, 
            ICacheManager cacheManager,
            ISignals signals) {
            _cacheManager = cacheManager;
            _shellSettings = shellSettings;
            _pca = pca;
            _signals = signals;

            var region = shellSettings.Name;

            // Azure Cache supports only alphanumeric strings for regions, but Orchard supports some
            // non-alphanumeric characters in tenant names. Remove all non-alphanumering characters
            // from the region, and append the hash code of the original string to mitigate the risk
            // of two distinct original region strings yielding the same transformed region string.
            _regionAlphaNumeric = new String(Array.FindAll(region.ToCharArray(), Char.IsLetterOrDigit)) + region.GetHashCode().ToString(CultureInfo.InvariantCulture);

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public CacheClientConfiguration CacheConfiguration {
            get {
                return _cacheManager.Get(ClientConfigurationKey, ctx => {
                    CacheClientConfiguration cacheConfig ;
                    try {
                        cacheConfig = CacheClientConfiguration.FromPlatformConfiguration(
                            _shellSettings.Name, 
                            Constants.OutputCacheSettingNamePrefix, 
                            _pca);

                        cacheConfig.Validate();
                        return cacheConfig;
                    }
                    catch (Exception ex) {
                        throw new Exception(String.Format("The {0} configuration settings are missing or invalid.", Constants.OutputCacheFeatureName), ex);
                    } 
                });
            }
        }

        public DataCache Cache {
            get {
                return _cacheManager.Get(DataCacheKey, ctx => {
                    ctx.Monitor(_signals.When(DataCacheKey));
                    var cache = CacheConfiguration.CreateCache();
                    cache.CreateRegion(_regionAlphaNumeric);

                    return cache;
                });
            }
        }

        public T SafeCall<T>(Func<T> function) {
            return Retry(function, Retries);
        }

        public void SafeCall(Action function) {
            Retry<object>(() => {
                function();
                return null;
            }, Retries);
        }

        public T Retry<T>(Func<T> function, int times) {
            if (times == 0) {
                Logger.Error("Too many retries in cache resolution.");
                return default(T);
            }

            try {
                return function.Invoke();
            }
            catch (DataCacheException) {
                _signals.Trigger(DataCacheKey);
                return Retry(function, times--);
            }
        } 

        public void Set(string key, CacheItem cacheItem) {
            if (cacheItem.ValidFor <= 0) {
                return;
            }

            Logger.Debug("Set() invoked with key='{0}' in region '{1}'.", key, _regionAlphaNumeric);
            SafeCall(() => Cache.Put(key, cacheItem, TimeSpan.FromSeconds(cacheItem.ValidFor), _regionAlphaNumeric));
        }

        public void Remove(string key) {
            Logger.Debug("Remove() invoked with key='{0}' in region '{1}'.", key, _regionAlphaNumeric);
            SafeCall(() => Cache.Remove(key, _regionAlphaNumeric));
        }

        public void RemoveAll() {
            Logger.Debug("RemoveAll() invoked in region '{0}'.", _regionAlphaNumeric);
            SafeCall<object>(() => {
                Cache.ClearRegion(_regionAlphaNumeric); 
                return null;
            });
        }

        public CacheItem GetCacheItem(string key) {
            Logger.Debug("GetCacheItem() invoked with key='{0}' in region '{1}'.", key, _regionAlphaNumeric);
            return SafeCall(() => Cache.Get(key, _regionAlphaNumeric)) as CacheItem;
        }

        public IEnumerable<CacheItem> GetCacheItems(int skip, int count) {
            Logger.Debug("GetCacheItems() invoked in region '{0}'.", _regionAlphaNumeric);
            return SafeCall(() => 
                Cache.GetObjectsInRegion(_regionAlphaNumeric)
                .AsParallel()
                .Select(x => x.Value)
                .OfType<CacheItem>()
                .Skip(skip)
                .Take(count)
                .ToArray()
                );
        }

        public int GetCacheItemsCount() {
            Logger.Debug("GetCacheItemsCount() invoked in region '{0}'.", _regionAlphaNumeric);
            return SafeCall(() => 
                Cache.GetObjectsInRegion(_regionAlphaNumeric).AsParallel()
                .Select(x => x.Value)
                .OfType<CacheItem>()
                .Count()
                );
        }
    }
}