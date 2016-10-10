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
        private readonly string _tenantName;
        private readonly WorkContext _workContext;

        public DefaultTagCache(IWorkContextAccessor workContextAccessor, ShellSettings shellSettings) {
            _workContext = workContextAccessor.GetContext();
            _tenantName = shellSettings.Name;
        }

        public void Tag(string tag, params string[] keys) {
            var set = Dictionary.GetOrAdd(tag, x => new HashSet<string>());

            lock (set) {
                foreach (var key in keys) {
                    set.Add(key);
                }
            }
        }

        public IEnumerable<string> GetTaggedItems(string tag) {
            HashSet<string> set;
            if (Dictionary.TryGetValue(tag, out set)) {
                lock (set) {
                    return set.ToReadOnlyCollection();
                }
            }

            return Enumerable.Empty<string>();
        }

        public void RemoveTag(string tag) {
            HashSet<string> set;
            Dictionary.TryRemove(tag, out set);
        }

        private ConcurrentDictionary<string, HashSet<string>> Dictionary {
            get {
                var key = _tenantName + ":TagCache";
                var dictionary = _workContext.HttpContext.Cache.Get(key) as ConcurrentDictionary<string, HashSet<string>>;

                if (dictionary == null) {
                    dictionary = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                    _workContext.HttpContext.Cache.Add(key, dictionary, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                }

                return dictionary;
            }
        }
    }
}