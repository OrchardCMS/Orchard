using System.Collections.Generic;

namespace Orchard.Search.ViewModels {
    public class SearchFormViewModel {
        public bool OverrideIndex { get; set; }
        public string SelectedIndex { get; set; }
        public IList<string> AvailableIndexes { get; set; }
    }
}