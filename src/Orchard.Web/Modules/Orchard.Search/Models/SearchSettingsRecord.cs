using Orchard.ContentManagement.Records;

namespace Orchard.Search.Models {
    public class SearchSettingsRecord : ContentPartRecord {
        public virtual bool FilterCulture { get; set; }
    }
}