using System.Collections.Generic;

namespace Orchard.Search.ViewModels {
    public class SearchSettingsViewModel {
        public IList<SearchSettingsEntry> Entries { get; set; }
        public bool FilterCulture { get; set; }
    }

    public class SearchSettingsEntry {
        public string Field { get; set; }
        public bool Selected { get; set; }
        public int Weight { get; set; }
    }
}