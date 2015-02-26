using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate.Util;
using Orchard.Environment.Extensions;
using Orchard.OutputCache.Services;
using Orchard.Redis.Configuration;
using StackExchange.Redis;

namespace Orchard.Redis.OutputCache
{
    [OrchardFeature("Orchard.Redis.OutputCache")]
    [OrchardSuppressDependency("Orchard.OutputCache.Services.DefaultTagCache")]
    public class RedisTagCache : ITagCache {
        private readonly IRedisConnectionProvider _redisConnectionProvider;
        private readonly string _connectionString;

        public RedisTagCache(IRedisConnectionProvider redisConnectionProvider) {
            _redisConnectionProvider = redisConnectionProvider;
            _connectionString = _redisConnectionProvider.GetConnectionString(RedisOutputCacheStorageProvider.ConnectionStringKey);
        }

        private IDatabase Database {
            get { return _redisConnectionProvider.GetConnection(_connectionString).GetDatabase(); }
        }

        public void Tag(string tag, params string[] keys) {
            Database.SetAdd(tag,  Array.ConvertAll(keys, x=> (RedisValue) x));
        }

        public IEnumerable<string> GetTaggedItems(string tag) {
            var values = Database.SetMembers(tag);
            if (values == null || values.Length == 0)
                return Enumerable.Empty<string>();
            return Array.ConvertAll(values, x => (string) x);
        }

        public void RemoveTag(string tag) {
            Database.KeyDelete(tag);
        }
    }
}