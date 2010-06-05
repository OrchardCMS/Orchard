using System.Collections.Generic;
using Orchard.Mvc.ViewModels;

namespace Orchard.Search.ViewModels {
    public class SearchViewModel : BaseViewModel {
        public IEnumerable<SearchResultViewModel> ResultsPage { get; set; }
        public int Count { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPageCount { get; set; }
        public string Query { get; set; }
    }
}