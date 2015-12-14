using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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

        private CacheClientConfiguration _cacheClientConfiguration;
        private static ConcurrentDictionary<CacheClientConfiguration, DataCacheFactory> _dataCacheFactories = new ConcurrentDictionary<CacheClientConfiguration, DataCacheFactory>();
        private static ConcurrentBag<string> _regions = new ConcurrentBag<string>();
        
        private readonly string _regionAlphaNumeric;
        private readonly ICacheManager _cacheManager;
        private readonly ShellSettings _shellSettings;
        private readonly IPlatformConfigurationAccessor _pca;

        public AzureOutputCacheStorageProvider(
            ShellSettings shellSettings, 
            IPlatformConfigurationAccessor pca, 
            ICacheManager cacheManager) {
            _cacheManager = cacheManager;
            _shellSettings = shellSettings;
            _pca = pca;

            var region = shellSettings.Name;

            // Azure Cache supports only alphanumeric strings for regions, but Orchard supports some
            // non-alphanumeric characters in tenant names. Remove all non-alphanumering characters
            // from the region, and append the hash code of the original string to mitigate the risk
            // of two distinct original region strings yielding the same transformed region string.
            _regionAlphaNumeric = new String(Array.FindAll(region.ToCharArray(), Char.IsLetterOrDigit)) + region.GetHashCode().ToString(CultureInfo.InvariantCulture);

            Logger = NullLogger.Instance;
        }

        public CacheClientConfiguration CacheConfiguration {
            get {
                // the configuration is backed by a field so that we don't call the cacheManager multiple times in the same request
                // cache configurations are stored in the cacheManager so that we don't read the config on each request
                if (_cacheClientConfiguration == null) {

                    _cacheClientConfiguration = _cacheManager.Get(ClientConfigurationKey, ctx => {
                        CacheClientConfiguration cacheConfig;
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

                    if (_cacheClientConfiguration == null) {
                        throw new InvalidOperationException("Could not create a valid cache configuration");
                    }
                }

                return _cacheClientConfiguration;
            }
        }

        public DataCache Cache {
            get {

                var cacheFactory = _dataCacheFactories.GetOrAdd(CacheConfiguration, cfg => {
                    Logger.Debug("Creating a new cache client ({0})", CacheConfiguration.GetHashCode());
                    return cfg.CreateCache();
                });

                var cache = String.IsNullOrEmpty(CacheConfiguration.CacheName) ? cacheFactory.GetDefaultCache() : cacheFactory.GetCache(CacheConfiguration.CacheName);

                // creating a region uses a network call, try to optimise it
                if (!_regions.Contains(_regionAlphaNumeric)) {
                    Logger.Debug("Creating a new region: {0}", _regionAlphaNumeric);
                    cache.CreateRegion(_regionAlphaNumeric);
                    _regions.Add(_regionAlphaNumeric);
                }

                return cache;
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
                // applying http://blogs.msdn.com/b/cie/archive/2014/04/29/cache-retry-fails-what-next.aspx
                try {
                    DataCacheFactory cacheFactory;
                    Logger.Debug("Retrying cache operation");
                    if (_dataCacheFactories.TryRemove(CacheConfiguration, out cacheFactory)) {
                        lock (_dataCacheFactories) {
                            cacheFactory.Dispose();
                            // Clear DataCacheFactory._connectionPool
                            var coreAssembly = typeof(DataCacheItem).Assembly;
                            var simpleSendReceiveModulePoolType = coreAssembly.
                                GetType("Microsoft.ApplicationServer.Caching.SimpleSendReceiveModulePool", throwOnError: true);
                            var connectionPoolField = typeof(DataCacheFactory).GetField("_connectionPool", BindingFlags.Static | BindingFlags.NonPublic);
                            connectionPoolField.SetValue(null, Activator.CreateInstance(simpleSendReceiveModulePoolType));
                        }
                    }
                    return Retry(function, times--);
                }
                catch (Exception e) {
                    Logger.Error(e, "An unexpected error occured while releasing a DataCacheFactory.");
                    return default(T);
                }
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