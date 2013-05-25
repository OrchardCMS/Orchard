using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Services {
    /// <summary>
    /// Tenant wide case insensitive reverse index for <see cref="CacheItem"/> tags.
    /// </summary>
    public class DefaultTagCache : ConcurrentDictionary<string, Collection<string>>, ITagCache {
        public DefaultTagCache() : base(StringComparer.OrdinalIgnoreCase) {
        }

        public void Tag(string tag, params string[] keys) {
            var collection = GetOrAdd(tag, x => new Collection<string>());

            foreach (var key in keys) {
                if (!collection.Contains(key)) {
                    collection.Add(key);
                }
            }
        }
    }
}