using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.OutputCache.Services {
    public interface IAsyncTagCache : ISingletonDependency {
        Task TagAsync(string tag, params string[] keys);
        Task<IEnumerable<string>> GetTaggedItemsAsync(string tag);
        Task RemoveTagAsync(string tag);
    }
}