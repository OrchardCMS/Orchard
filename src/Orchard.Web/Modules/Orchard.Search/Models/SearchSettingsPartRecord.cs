using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.Search.Models {
    public class SearchSettingsPartRecord : ContentPartRecord {
        public SearchSettingsPartRecord() {
            FilterCulture = false;
            SearchedFields = "body, title";
        }
        
        public virtual bool FilterCulture { get; set; }
        
        [StringLengthMax]
        public virtual string SearchedFields { get; set; }
        public virtual string SearchIndex { get; set; }
    }
}