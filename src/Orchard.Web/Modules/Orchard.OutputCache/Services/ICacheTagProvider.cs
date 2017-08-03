using System.Collections.Generic;

namespace Orchard.OutputCache.Services {
    public interface ICacheTagProvider : IDependency {
        IEnumerable<string> GetTags();
    }
}