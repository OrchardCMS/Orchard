using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Orchard.Utility.Extensions;
using Orchard.Environment.Configuration;
using System.Web.Caching;

namespace Orchard.OutputCache.Services {
    /// <summary>
    /// Tenant wide case insensitive reverse index for <see cref="CacheItem"/> tags.
    /// </summary>
    public class DefaultTagCache : ITagCache {
        private readonly ConcurrentDictionary<string, HashSet<string>> _dictionary;

        public DefaultTagCache(IWorkContextAccessor workContextAccessor, ShellSettings shellSettings) {
            var key = shellSettings.Name + ":TagCache";
            var workContext = workContextAccessor.GetContext();

            if ( workContext != null ) {
                _dictionary = workContext.HttpContext.Cache.Get(key) as ConcurrentDictionary<string, HashSet<string>>;

                if ( _dictionary == null ) {
                    _dictionary = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                    workContext.HttpContext.Cache.Add(key, _dictionary, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                }
            }
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