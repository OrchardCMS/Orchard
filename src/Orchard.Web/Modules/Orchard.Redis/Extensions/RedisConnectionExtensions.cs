using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;

namespace Orchard.Redis.Extensions {
    public static class RedisConnectionExtensions {
        public static void KeyDeleteWithPrefix(this IConnectionMultiplexer connection, string prefix) {
            var keys = GetKeys(connection, prefix);
            var database = connection.GetDatabase();
            database.KeyDelete(keys.Select(key => (RedisKey)key).ToArray());
        }

        public static int KeyCount(this IConnectionMultiplexer connection, string prefix) {
            return GetKeys(connection, prefix).Count();
        }

        // CYCLES EACH ENDPOINT FOR A CONNECTION SO KEYS ARE FETECHED IN A REDIS CLUSTER CONFIGURATION
        // STACKEXCHANGECLIENT .KEYS WILL PERFORM SCAN ON REDIS SERVERS THAT SUPPORT IT
        public static IEnumerable<string> GetKeys(this IConnectionMultiplexer connection, string prefix) {
            if (connection == null) {
                throw new ArgumentException("Connection cannot be null", "connection");
            }

            if (string.IsNullOrWhiteSpace(prefix)) {
                throw new ArgumentException("Prefix cannot be empty", "database");
            }

            var keys = new List<string>();
            var databaseId = connection.GetDatabase().Database;

            foreach (var endPoint in connection.GetEndPoints()) {
                var server = connection.GetServer(endPoint);
                keys.AddRange(server.Keys(pattern: prefix, database: databaseId).Select(x => x.ToString()));
            }

            return keys.Distinct();
        }
    }
}