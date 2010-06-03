using System.Collections.Generic;
using Orchard.Indexing;

namespace Orchard.Search.Services {
    public interface ISearchService : IDependency {
        IEnumerable<ISearchHit> Query(string term);
    }
}