using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Orchard.Search.Models {
    [OrchardFeature("Orchard.Search.Content")]
    public class AdminSearchSettingsPart : ContentPart {
        
        public string SearchIndex {
            get { return this.Retrieve(x => x.SearchIndex); }
            set { this.Store(x => x.SearchIndex, value); }
        }
    }
}
