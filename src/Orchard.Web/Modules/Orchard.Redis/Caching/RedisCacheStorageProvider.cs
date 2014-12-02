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

        public IDatabase Database {
            get {
                return _redisConnectionProvider.GetConnection(_connectionString).GetDatabase();
            }
        }

        public RedisCacheStorageProvider(ShellSettings shellSettings, IRedisConnectionProvider redisConnectionProvider) {
            _shellSettings = shellSettings;
            _redisConnectionProvider = redisConnectionProvider;
            _connectionString = _redisConnectionProvider.GetConnectionString(ConnectionStringKey);
            Logger = NullLogger.Instance;
        }

        public object Get<T>(string key) {
            var json = Database.StringGet(GetLocalizedKey(key));
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
            Database.KeyDelete(key);
        }

        public void Clear() {
            Database.KeyDeleteWithPrefix(GetLocalizedKey("*"));
        }

        private string GetLocalizedKey(string key) {
            return _shellSettings.Name + ":Cache:" + key;
        }
    }
}
