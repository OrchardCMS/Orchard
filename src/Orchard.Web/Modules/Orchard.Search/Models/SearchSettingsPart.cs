using System;
using Orchard.ContentManagement;

namespace Orchard.Search.Models {
    public class SearchSettingsPart : ContentPart {
        public string[] SearchedFields {
            get { return (Retrieve<string>("SearchedFields") ?? "").Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);  }
            set { Store("SearchedFields", String.Join(", ", value));  }
        }

        public bool FilterCulture {
            get { return this.Retrieve(x => x.FilterCulture); }
            set { this.Store(x => x.FilterCulture, value); }
        }

        public string SearchIndex {
            get { return this.Retrieve(x => x.SearchIndex); }
            set { this.Store(x => x.SearchIndex, value); }
        }
    }
}
