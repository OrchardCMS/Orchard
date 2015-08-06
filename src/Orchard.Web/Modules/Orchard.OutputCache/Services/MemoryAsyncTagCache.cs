using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orchard.Environment.Extensions;
using Orchard.Utility.Extensions;

namespace Orchard.OutputCache.Services {
    /// <summary>
    /// Tenant wide case insensitive reverse index for <see cref="CacheItem"/> tags.
    /// </summary>
    [OrchardFeature("Orchard.OutputCache2")]
    public class MemoryAsyncTagCache : IAsyncTagCache {

        private readonly ConcurrentDictionary<string, HashSet<string>> _dictionary;

        public MemoryAsyncTagCache() {
            _dictionary = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task TagAsync(string tag, params string[] keys) {
            var set = _dictionary.GetOrAdd(tag, x => new HashSet<string>());

            lock (set) {
                foreach (var key in keys) {
                    set.Add(key);
                }
            }
        }
        
        public async Task<IEnumerable<string>> GetTaggedItemsAsync(string tag) {
            HashSet<string> set;
            if (_dictionary.TryGetValue(tag, out set)) {
                lock (set) {
                    return set.ToReadOnlyCollection();
                }
            }

            return Enumerable.Empty<string>();
        }

        public async Task RemoveTagAsync(string tag) {
            HashSet<string> set;
            _dictionary.TryRemove(tag, out set);
        }
    }
}