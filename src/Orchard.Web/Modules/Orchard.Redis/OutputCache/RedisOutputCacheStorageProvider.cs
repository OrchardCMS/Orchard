using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Redis.Configuration;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;
using Orchard.Redis.Extensions;
using StackExchange.Redis;

namespace Orchard.Redis.OutputCache {

    [OrchardFeature("Orchard.Redis.OutputCache")]
    [OrchardSuppressDependency("Orchard.OutputCache.Services.DefaultCacheStorageProvider")]
    public class RedisOutputCacheStorageProvider : Component, IOutputCacheStorageProvider {

        private readonly ShellSettings _shellSettings;
        private readonly IRedisConnectionProvider _redisConnectionProvider;
        private HashSet<string> _keysCache;

        public const string ConnectionStringKey = "Orchard.Redis.OutputCache";
        private readonly string _connectionString;

        public RedisOutputCacheStorageProvider(ShellSettings shellSettings, IRedisConnectionProvider redisConnectionProvider) {
            _shellSettings = shellSettings;
            _redisConnectionProvider = redisConnectionProvider;
            _connectionString = _redisConnectionProvider.GetConnectionString(ConnectionStringKey);

            Logger = NullLogger.Instance;
        }

        public IDatabase Database {
            get {
                return _redisConnectionProvider.GetConnection(_connectionString).GetDatabase();
            }
        }

        public void Set(string key, CacheItem cacheItem) {
            if (cacheItem.ValidFor <= 0) {
                return;
            }

            var value = JsonConvert.SerializeObject(cacheItem);
            Database.StringSet(GetLocalizedKey(key), value, TimeSpan.FromSeconds(cacheItem.ValidFor));
        }

        public void Remove(string key) {
            Database.KeyDelete(GetLocalizedKey(key));
        }

        public void RemoveAll() {
            Database.KeyDeleteWithPrefix(GetLocalizedKey("*"));
        }

        public CacheItem GetCacheItem(string key) {
            string value = Database.StringGet(GetLocalizedKey(key));
            if (String.IsNullOrEmpty(value)) {
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
            return Database.KeyCount(GetLocalizedKey("*"));
        }

        /// <summary>
        /// Creates a namespaced key to support multiple tenants on top of a single Redis connection.
        /// </summary>
        /// <param name="key">The key to localized.</param>
        /// <returns>A localized key based on the tenant name.</returns>
        private string GetLocalizedKey(string key) {
            return _shellSettings.Name + ":OutputCache:" + key;
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

                var connection = _redisConnectionProvider.GetConnection(_connectionString);
                foreach (var endPoint in connection.GetEndPoints()) {
                    var server = connection.GetServer(endPoint);
                    foreach (var key in server.Keys(pattern: GetLocalizedKey("*"))) {
                        _keysCache.Add(key.ToString().Substring(prefix.Length));
                    }
                }
            }

            return _keysCache;
        }
    }
}