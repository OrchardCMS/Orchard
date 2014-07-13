using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.ApplicationServer.Caching;
using Newtonsoft.Json;
using Orchard.Azure.Services.Environment.Configuration;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;
using StackExchange.Redis;

namespace Orchard.Azure.Services.Caching.Output {

    [OrchardFeature("Orchard.Azure.RedisOutputCache")]
    [OrchardSuppressDependency("Orchard.OutputCache.Services.DefaultCacheStorageProvider")]
    public class AzureRedisCacheStorageProvider : Component, IOutputCacheStorageProvider {
        public const string DataCacheKey = "DataCache";
        public const string ClientConfigurationKey = "CacheClientConfiguration";

        private CacheClientConfiguration _cacheClientConfiguration;
        private static ConcurrentDictionary<CacheClientConfiguration, ConnectionMultiplexer> _connectionMultiplexers = new ConcurrentDictionary<CacheClientConfiguration, ConnectionMultiplexer>();
        
        private readonly ICacheManager _cacheManager;
        private readonly ShellSettings _shellSettings;
        private readonly IPlatformConfigurationAccessor _pca;

        public AzureRedisCacheStorageProvider(
            ShellSettings shellSettings, 
            IPlatformConfigurationAccessor pca, 
            ICacheManager cacheManager) {
            _cacheManager = cacheManager;
            _shellSettings = shellSettings;
            _pca = pca;

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

        public IDatabase Cache {
            get {

                var connectionMultiplexer = _connectionMultiplexers.GetOrAdd(CacheConfiguration, cfg => {
                    Logger.Debug("Creating a new cache client ({0})", CacheConfiguration.GetHashCode());
                    var connectionString = cfg.HostIdentifier + ",password=" + cfg.AuthorizationToken;
                    return ConnectionMultiplexer.Connect(connectionString);
                });
                
                return connectionMultiplexer.GetDatabase();
            }
        }

        public void Set(string key, CacheItem cacheItem) {
            if (cacheItem.ValidFor <= 0) {
                return;
            }

            var value = JsonConvert.SerializeObject(cacheItem);
            Cache.StringSet(key, value, TimeSpan.FromSeconds(cacheItem.ValidFor));
        }

        public void Remove(string key) {
            Cache.StringSet(key, String.Empty, TimeSpan.MinValue);
        }

        public void RemoveAll() {
        }

        public CacheItem GetCacheItem(string key) {
            string value = Cache.StringGet(key);
            if(String.IsNullOrEmpty(value)) {
                return null;
            }

            return JsonConvert.DeserializeObject<CacheItem>(value);
        }

        public IEnumerable<CacheItem> GetCacheItems(int skip, int count) {

            return Enumerable.Empty<CacheItem>();
        }

        public int GetCacheItemsCount() {
            return 0;
        }
    }
}