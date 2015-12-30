using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Orchard.OutputCache.Models;
using Orchard.Utility.Extensions;

namespace Orchard.OutputCache.Services {
    /// <summary>
    /// Tenant wide case insensitive reverse index for <see cref="CacheItem"/> tags.
    /// </summary>
    public class DefaultTagCache : ITagCache {

        private readonly ConcurrentDictionary<string, HashSet<string>> _dictionary;

        public DefaultTagCache() {
            _dictionary = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        }

        public void Tag(string tag, params string[] keys) {
            var set = _dictionary.GetOrAdd(tag, x => new HashSet<string>());

            lock (set) {
                foreach (var key in keys) {
                    set.Add(key);
                }
            }
        }
        
        public IEnumerable<string> GetTaggedItems(string tag) {
            HashSet<string> set;
            if (_dictionary.TryGetValue(tag, out set)) {
                lock (set) {
                    return set.ToReadOnlyCollection();
                }
            }

            return Enumerable.Empty<string>();
        }

        public void RemoveTag(string tag) {
            HashSet<string> set;
            _dictionary.TryRemove(tag, out set);
        }
    }
}