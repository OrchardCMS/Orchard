using System.Collections.Generic;

namespace Orchard.OutputCache.Services {
    public interface ITagCache : ISingletonDependency {
        void Tag(string tag, params string[] keys);
        IEnumerable<string> GetTaggedItems(string tag);
        void RemoveTag(string tag);
    }
}