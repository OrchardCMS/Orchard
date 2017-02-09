using Newtonsoft.Json;
using Orchard.Caching.Services;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Redis.Configuration;
using Orchard.Redis.Extensions;
using StackExchange.Redis;
using System;

namespace Orchard.Redis.Caching {

    [OrchardFeature("Orchard.Redis.Caching")]
    [OrchardSuppressDependency("Orchard.Caching.Services.DefaultCacheStorageProvider")]
    public class RedisCacheStorageProvider : Component, ICacheStorageProvider {
        public const string ConnectionStringKey = "Orchard.Redis.Cache";

        private readonly ShellSettings _shellSettings;
        private readonly IRedisConnectionProvider _redisConnectionProvider;
        private readonly string _connectionString;
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public IDatabase Database {
            get {
                return _connectionMultiplexer.GetDatabase();
            }
        }

        public RedisCacheStorageProvider(ShellSettings shellSettings, IRedisConnectionProvider redisConnectionProvider) {
            _shellSettings = shellSettings;
            _redisConnectionProvider = redisConnectionProvider;
            _connectionString = _redisConnectionProvider.GetConnectionString(ConnectionStringKey);
            _connectionMultiplexer = _redisConnectionProvider.GetConnection(_connectionString);

            Logger = NullLogger.Instance;
        }

        public object Get<T>(string key) {
            var json = Database.StringGet(GetLocalizedKey(key));
            if(String.IsNullOrEmpty(json)) {
                return null;
            }
            return JsonConvert.DeserializeObject<T>(json);
        }

        public void Put<T>(string key, T value) {
            var json = JsonConvert.SerializeObject(value);
            Database.StringSet(GetLocalizedKey(key), json, null);
        }

        public void Put<T>(string key, T value, TimeSpan validFor) {
            var json = JsonConvert.SerializeObject(value);
            Database.StringSet(GetLocalizedKey(key), json, validFor);
        }

        public void Remove(string key) {
            Database.KeyDelete(GetLocalizedKey(key));
        }

        public void Clear() {
            _connectionMultiplexer.KeyDeleteWithPrefix(GetLocalizedKey("*"));
        }

        private string GetLocalizedKey(string key) {
            return _shellSettings.Name + ":Cache:" + key;
        }
    }
}
