using System.Collections.Generic;

namespace Orchard.Search.ViewModels {
    public class SearchSettingsViewModel {
        public string SelectedIndex { get; set; }
        public IList<IndexSettingsEntry> Entries { get; set; }
        public bool FilterCulture { get; set; }
    }

    public class IndexSettingsEntry {
        public string Index { get; set; }
        public IList<SearchSettingsEntry> Fields { get; set; }
    }

    public class SearchSettingsEntry {
        public string Field { get; set; }
        public bool Selected { get; set; }
        public int Weight { get; set; }
    }
}