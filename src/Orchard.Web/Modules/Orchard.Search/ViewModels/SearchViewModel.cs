using Orchard.Collections;

namespace Orchard.Search.ViewModels {
    public class SearchViewModel {
        public string Query { get; set; }
        public int DefaultPageSize { get; set; }
        public IPageOfItems<SearchResultViewModel> PageOfResults { get; set; }
    }
}