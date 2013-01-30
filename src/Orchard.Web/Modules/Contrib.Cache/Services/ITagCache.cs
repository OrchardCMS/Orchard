using System.Collections.Generic;
using System.Collections.ObjectModel;
using Orchard;

namespace Contrib.Cache.Services {
    public interface ITagCache : IDictionary<string, Collection<string>>, ISingletonDependency {
        void Tag(string tag, params string[] keys);
    }
}