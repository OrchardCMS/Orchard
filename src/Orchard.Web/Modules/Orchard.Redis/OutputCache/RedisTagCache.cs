using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.OutputCache.Services;
using Orchard.Redis.Configuration;
using StackExchange.Redis;

namespace Orchard.Redis.OutputCache {
    [OrchardFeature("Orchard.Redis.OutputCache")]
    [OrchardSuppressDependency("Orchard.OutputCache.Services.DefaultTagCache")]
    public class RedisTagCache : ITagCache {
        private readonly IRedisConnectionProvider _redisConnectionProvider;
        private readonly string _connectionString;
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly ShellSettings _shellSettings;

        public RedisTagCache(IRedisConnectionProvider redisConnectionProvider, ShellSettings shellSettings) {
            _redisConnectionProvider = redisConnectionProvider;
            _connectionString = _redisConnectionProvider.GetConnectionString(RedisOutputCacheStorageProvider.ConnectionStringKey);
            _connectionMultiplexer = _redisConnectionProvider.GetConnection(_connectionString);
            _shellSettings = shellSettings;
        }

        private IDatabase Database {
            get { return _connectionMultiplexer.GetDatabase(); }
        }

        public void Tag(string tag, params string[] keys) {
            Database.SetAdd(GetLocalizedKey(tag),  Array.ConvertAll(keys, x=> (RedisValue) x));
        }

        public IEnumerable<string> GetTaggedItems(string tag) {
            var values = Database.SetMembers(GetLocalizedKey(tag));
            if (values == null || values.Length == 0)
                return Enumerable.Empty<string>();
            return Array.ConvertAll(values, x => (string) x);
        }

        public void RemoveTag(string tag) {
            Database.KeyDelete(GetLocalizedKey(tag));
        }

        private string GetLocalizedKey(string key) {
            return _shellSettings.Name + ":Tag:" + key;
        }
    }
}