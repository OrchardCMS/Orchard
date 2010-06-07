using Orchard.Collections;
using Orchard.Mvc.ViewModels;

namespace Orchard.Search.ViewModels {
    public class SearchViewModel : BaseViewModel {
        public string Query { get; set; }
        public int DefaultPageSize { get; set; }
        public IPageOfItems<SearchResultViewModel> PageOfResults { get; set; }
    }
}