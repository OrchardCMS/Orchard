using System.Collections.Generic;
using Orchard.Indexing;

namespace Orchard.Search.Models {
    public class SearchResult : ISearchResult {
        public IEnumerable<ISearchHit> Page { get; set; }
        public int TotalCount { get; set; }
    }
}