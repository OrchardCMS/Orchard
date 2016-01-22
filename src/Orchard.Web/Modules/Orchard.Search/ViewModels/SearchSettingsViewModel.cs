using System.Collections.Generic;

namespace Orchard.Search.ViewModels {
    public class SearchSettingsViewModel {
        public SearchSettingsViewModel() {
            Entries = new List<IndexSettingsEntry>();
        }

        public string SelectedIndex { get; set; }
        public IList<IndexSettingsEntry> Entries { get; set; }
        public bool FilterCulture { get; set; }
        public string DisplayType { get; set; }
    }

    public class IndexSettingsEntry {
        public IndexSettingsEntry() {
            Fields = new List<SearchSettingsEntry>();
        }

        public string Index { get; set; }
        public IList<SearchSettingsEntry> Fields { get; set; }
    }

    public class SearchSettingsEntry {
        public string Field { get; set; }
        public bool Selected { get; set; }
        public int Weight { get; set; }
    }
}