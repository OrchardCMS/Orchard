using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.OutputCache.Services;
using Orchard.Redis.Configuration;
using StackExchange.Redis;

namespace Orchard.Redis.OutputCache {
    [OrchardFeature("Orchard.Redis.OutputCache2")]
    [OrchardSuppressDependency("Orchard.OutputCache.Services.MemoryAsyncTagCache")]
    public class RedisAsyncTagCache : IAsyncTagCache {
        private readonly IRedisConnectionProvider _redisConnectionProvider;
        private readonly string _connectionString;
        private readonly ConnectionMultiplexer _connectionMultiplexer;
        private readonly ShellSettings _shellSettings;

        public RedisAsyncTagCache(IRedisConnectionProvider redisConnectionProvider, ShellSettings shellSettings) {
            _redisConnectionProvider = redisConnectionProvider;
            _connectionString = _redisConnectionProvider.GetConnectionString(RedisOutputCacheStorageProvider.ConnectionStringKey);
            _connectionMultiplexer = _redisConnectionProvider.GetConnection(_connectionString);
            _shellSettings = shellSettings;
        }

        private IDatabase Database {
            get { return _connectionMultiplexer.GetDatabase(); }
        }

        public async Task TagAsync(string tag, params string[] keys) {
            await Database.SetAddAsync(GetLocalizedKey(tag),  Array.ConvertAll(keys, x=> (RedisValue) x));
        }

        public async Task<IEnumerable<string>> GetTaggedItemsAsync(string tag) {
            var values = await Database.SetMembersAsync(GetLocalizedKey(tag));
            if (values == null || values.Length == 0)
                return Enumerable.Empty<string>();
            return Array.ConvertAll(values, x => (string) x);
        }

        public async Task RemoveTagAsync(string tag) {
            await Database.KeyDeleteAsync(GetLocalizedKey(tag));
        }

        private string GetLocalizedKey(string key) {
            return _shellSettings.Name + ":Tag:" + key;
        }
    }
}