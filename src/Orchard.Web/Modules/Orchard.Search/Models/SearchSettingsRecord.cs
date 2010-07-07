using Orchard.ContentManagement.Records;

namespace Orchard.Search.Models {
    public class SearchSettingsRecord : ContentPartRecord {
        public virtual bool FilterCulture { get; set; }
        public virtual string SearchedFields { get; set; }
        
        public SearchSettingsRecord() {
            FilterCulture = false;
            SearchedFields = "body, title";
        }
    }
}