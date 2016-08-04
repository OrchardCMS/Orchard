using Orchard.ContentManagement;
using Orchard.Indexing;

namespace Orchard.Search.ViewModels {
    public class SearchResultViewModel {
        public ISearchHit SearchHit { get; set; }
        public IContent Content { get; set; }
    }
}