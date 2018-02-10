using Orchard.ContentManagement.Records;

namespace Orchard.ContentPicker.Models {
    public class ContentMenuItemPartRecord : ContentPartRecord {
        public virtual ContentItemRecord ContentMenuItemRecord { get; set; }
    }
}