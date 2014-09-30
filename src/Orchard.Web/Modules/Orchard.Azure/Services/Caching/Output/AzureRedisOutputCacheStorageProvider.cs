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
        private HashSet<string> _keysCache;

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
                
                return GetConnection().GetDatabase();
            }
        }

        private ConnectionMultiplexer GetConnection() {
            var connectionMultiplexer = _connectionMultiplexers.GetOrAdd(CacheConfiguration, cfg => {
                Logger.Debug("Creating a new cache client ({0})", CacheConfiguration.GetHashCode());
                var connectionString = cfg.HostIdentifier + ",password=" + cfg.AuthorizationToken;
                return ConnectionMultiplexer.Connect(connectionString);
            });


            return connectionMultiplexer;
        }

        public void Set(string key, CacheItem cacheItem) {
            if (cacheItem.ValidFor <= 0) {
                return;
            }

            var value = JsonConvert.SerializeObject(cacheItem);
            Cache.StringSet(GetLocalizedKey(key), value, TimeSpan.FromSeconds(cacheItem.ValidFor));
        }
        
        public void Remove(string key) {
            Cache.KeyDelete(GetLocalizedKey(key));
        }

        public void RemoveAll() {
            foreach (var key in GetAllKeys()) {
                Remove(key);
            }
        }

        public CacheItem GetCacheItem(string key) {
            string value = Cache.StringGet(GetLocalizedKey(key));
            if(String.IsNullOrEmpty(value)) {
                return null;
            }

            return JsonConvert.DeserializeObject<CacheItem>(value);
        }

        public IEnumerable<CacheItem> GetCacheItems(int skip, int count) {
            foreach (var key in GetAllKeys().Skip(skip).Take(count)) {
                var cacheItem = GetCacheItem(key);
                // the item could have expired in the meantime
                if (cacheItem != null) {
                    yield return cacheItem;
                }
            }
        }

        public int GetCacheItemsCount() {
            return GetAllKeys().Count();
        }

        /// <summary>
        /// Creates a namespaced key to support multiple tenants on top of a single Redis connection.
        /// </summary>
        /// <param name="key">The key to localized.</param>
        /// <returns>A localized key based on the tenant name.</returns>
        private string GetLocalizedKey(string key) {
            return _shellSettings.Name + ":" + key;
        }

        /// <summary>
        /// Returns all the keys for the current tenant.
        /// </summary>
        /// <returns>The keys for the current tenant.</returns>
        private IEnumerable<string> GetAllKeys() {
            // prevent the same request from computing the list twice (count + list)
            if (_keysCache == null) {
                _keysCache = new HashSet<string>();
                var prefix = GetLocalizedKey("");

                var connection = GetConnection();
                foreach (var endPoint in connection.GetEndPoints()) {
                    var server = GetConnection().GetServer(endPoint);
                    foreach (var key in server.Keys(pattern: GetLocalizedKey("*"))) {
                        _keysCache.Add(key.ToString().Substring(prefix.Length));
                    }
                }
            }

            return _keysCache;
        }
    }
}