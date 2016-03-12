using System.Collections.Concurrent;
using System.Threading.Tasks;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Services {
    public class OutputCacheFilterState : IOutputCacheFilterState {
        public OutputCacheFilterState() {
            Cachers = new ConcurrentDictionary<string, TaskCompletionSource<CacheItem>>();
            Renderers = new ConcurrentDictionary<string, TaskCompletionSource<CacheItem>>();
        }

        public ConcurrentDictionary<string, TaskCompletionSource<CacheItem>> Cachers { get; private set; }

        public ConcurrentDictionary<string, TaskCompletionSource<CacheItem>> Renderers { get; private set; }
    }
}