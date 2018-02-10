using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.Redis.Configuration;
using Orchard.OutputCache.Models;
using Orchard.OutputCache.Services;
using Orchard.Redis.Extensions;
using StackExchange.Redis;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.Compression;

namespace Orchard.Redis.OutputCache {

    [OrchardFeature("Orchard.Redis.OutputCache")]
    [OrchardSuppressDependency("Orchard.OutputCache.Services.DefaultCacheStorageProvider")]
    /// <summary>
    /// This implementation stores a <see cref="CacheItem"/> instance to a Redis server.
    /// The item is serialized using a <see cref="BinaryFormatter"/> and GZipped. We rely
    /// on compression at this level of the implementation as other output cache providers
    /// might not want to rely on it, or transform the data to binary. The content is compressed
    /// as HTML pages can be consequent, like several hundreds of KB, and the network be clogged.
    /// To prevent versioning issues with serialized data, the Redis keys contain the 
    /// <see cref="CacheItem.Version"/> property. 
    /// </summary>
    public class RedisOutputCacheStorageProvider : IOutputCacheStorageProvider {

        private readonly ShellSettings _shellSettings;
        private readonly IRedisConnectionProvider _redisConnectionProvider;
        private HashSet<string> _keysCache;

        public const string ConnectionStringKey = "Orchard.Redis.OutputCache";
        private readonly string _connectionString;
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public RedisOutputCacheStorageProvider(ShellSettings shellSettings, IRedisConnectionProvider redisConnectionProvider) {
            _shellSettings = shellSettings;
            _redisConnectionProvider = redisConnectionProvider;
            _connectionString = _redisConnectionProvider.GetConnectionString(ConnectionStringKey);
            _connectionMultiplexer = _redisConnectionProvider.GetConnection(_connectionString);

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IDatabase Database {
            get {
                return _connectionMultiplexer.GetDatabase();
            }
        }

        public void Set(string key, CacheItem cacheItem) {
            if (_connectionMultiplexer == null) {
                return;
            }

            if (cacheItem == null) {
                throw new ArgumentNullException("cacheItem");
            }

            if (cacheItem.ValidFor <= 0) {
                return;
            }

            using (var decompressedStream = Serialize(cacheItem)) {
                using (var compressedStream = Compress(decompressedStream)) {
                    Database.StringSet(GetLocalizedKey(key), compressedStream.ToArray(), TimeSpan.FromSeconds(cacheItem.ValidFor));
                }
            }
        }

        public void Remove(string key) {
            if (_connectionMultiplexer == null) {
                return;
            }

            Database.KeyDelete(GetLocalizedKey(key));
        }

        public void RemoveAll() {
            if (_connectionMultiplexer == null) {
                return;
            }

            Database.KeyDelete(GetPrefixedKeys().Select(key => (RedisKey)key).ToArray());
        }

        public CacheItem GetCacheItem(string key) {
            if (_connectionMultiplexer == null) {
                return null;
            }

            var value = Database.StringGet(GetLocalizedKey(key));

            if (value.IsNullOrEmpty) {
                return null;
            }

            using (var compressedStream = new MemoryStream(value)) {
                if (compressedStream.Length == 0) {
                    return null;
                }

                using (var decompressedStream = Decompress(compressedStream)) {
                    return Deserialize(decompressedStream);
                }
            }
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
            if (_connectionMultiplexer == null) {
                return 0;
            }

            return GetPrefixedKeys().Count();
        }

        /// <summary>
        /// Creates a namespaced key to support multiple tenants on top of a single Redis connection.
        /// </summary>
        /// <param name="key">The key to localized.</param>
        /// <returns>A localized key based on the tenant name.</returns>
        private string GetLocalizedKey(string key) {
            return _shellSettings.Name + ":OC:" + CacheItem.Version + ":" + key;
        }

        /// <summary>
        /// Returns all the keys for the current tenant.
        /// </summary>
        /// <returns>The keys for the current tenant.</returns>
        private IEnumerable<string> GetAllKeys() {
            if (_connectionMultiplexer == null) {
                return new string[0];
            }

            var prefix = GetLocalizedKey("");
            return GetPrefixedKeys().Select(x => x.Substring(prefix.Length));
        }

        private IEnumerable<string> GetPrefixedKeys() {
            // prevent the same request from computing the list twice (count + list)
            if (_keysCache == null) {
                _keysCache = new HashSet<string>();
                var keys = _connectionMultiplexer.GetKeys(GetLocalizedKey("*"));
                foreach (var key in keys) {
                    _keysCache.Add(key);
                }
            }
            return _keysCache;
        }

        private static MemoryStream Serialize(CacheItem item) {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, item);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        private static CacheItem Deserialize(Stream stream) {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            var result = (CacheItem)binaryFormatter.Deserialize(stream);
            return result;
        }

        private static MemoryStream Compress(Stream stream) {
            var compressedStream = new MemoryStream();
            using (var compressionStream = new GZipStream(compressedStream, CompressionMode.Compress)) {
                stream.CopyTo(compressionStream);
                return compressedStream;
            }
        }

        private static Stream Decompress(Stream stream) {
            var decompressedStream = new MemoryStream();
            using (GZipStream decompressionStream = new GZipStream(stream, CompressionMode.Decompress)) {
                decompressionStream.CopyTo(decompressedStream);
                decompressedStream.Seek(0, SeekOrigin.Begin);
                return decompressedStream;
            }

        }
    }
}