using System.Collections.Generic;
using Orchard.Mvc.ViewModels;

namespace Orchard.Search.ViewModels {
    public class SearchViewModel : BaseViewModel {
        public IEnumerable<SearchResultViewModel> Results { get; set; }
        public string Query { get; set; }
    }
}