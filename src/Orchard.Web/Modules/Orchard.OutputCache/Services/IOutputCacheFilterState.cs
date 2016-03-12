using System.Collections.Concurrent;
using System.Threading.Tasks;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Services {
    public interface IOutputCacheFilterState : ISingletonDependency {
        
        /// <summary>
        /// Tasks that are currently rendering pages
        /// </summary>
        ConcurrentDictionary<string, TaskCompletionSource<CacheItem>> Renderers { get; }

        /// <summary>
        /// Tasks that are currently retrieving cached items
        /// </summary>
        ConcurrentDictionary<string, TaskCompletionSource<CacheItem>> Cachers { get; }

    }
}