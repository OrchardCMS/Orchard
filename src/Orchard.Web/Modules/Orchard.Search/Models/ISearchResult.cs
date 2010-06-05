using System.Collections.Generic;
using Orchard.Indexing;

namespace Orchard.Search.Models {
    public interface ISearchResult {
        IEnumerable<ISearchHit> Page { get; set; }
        int TotalCount { get; set; }
    }
}