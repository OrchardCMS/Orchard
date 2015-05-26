using System.Collections.Generic;

namespace Orchard.Search.ViewModels {
    public class AdminSearchSettingsViewModel {
        public AdminSearchSettingsViewModel() {
            AvailableIndexes = new List<string>();
        }

        public string SelectedIndex { get; set; }
        public IList<string> AvailableIndexes { get; set; }
    }
}