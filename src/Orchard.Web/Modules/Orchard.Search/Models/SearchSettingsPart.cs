using System;
using Orchard.ContentManagement;

namespace Orchard.Search.Models {
    public class SearchSettingsPart : ContentPart<SearchSettingsPartRecord> {
        public string[] SearchedFields {
            get { return Record.SearchedFields.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);  }
            set { Record.SearchedFields = String.Join(", ", value);  }
        }

        public bool FilterCulture {
            get { return Record.FilterCulture; }
            set { Record.FilterCulture = value; }
        }
    }
}
