using Orchard.Indexing;
using Orchard.Mvc.ViewModels;

namespace Orchard.Search.ViewModels {
    public class SearchResultViewModel {
        public ISearchHit SearchHit { get; set; }
        public ContentItemViewModel Content { get; set; }
    }
}