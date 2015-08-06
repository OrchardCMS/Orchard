using System.Collections.Generic;
using System.Threading.Tasks;
using Orchard.OutputCache.Models;

namespace Orchard.OutputCache.Services {
    public interface IAsyncOutputCacheStorageProvider : IDependency {
        Task<CacheItem> GetAsync(string key);
        Task SetAsync(string key, CacheItem cacheItem);
        Task RemoveAsync(string key);
        Task<IEnumerable<CacheItem>> GetAllAsync(int skip, int count);
        Task RemoveAllAsync();
        Task<int> CountAsync();
    }
}