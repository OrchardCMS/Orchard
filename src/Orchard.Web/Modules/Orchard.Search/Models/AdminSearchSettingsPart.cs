using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Search.Helpers;

namespace Orchard.Search.Models {
    [OrchardFeature("Orchard.Search.Content")]
    public class AdminSearchSettingsPart : ContentPart {
        public IDictionary<string, string[]> SearchFields {
            get {
                var data = Retrieve<string>("SearchFields") ?? "Admin:body,title";
                return SearchSettingsHelper.DeserializeSearchFields(data);
            }
            set {
                var data = SearchSettingsHelper.SerializeSearchFields(value);
                Store("SearchFields", data);
            }
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
