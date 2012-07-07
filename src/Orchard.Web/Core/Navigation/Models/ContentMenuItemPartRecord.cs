using Orchard.ContentManagement.Records;

namespace Orchard.Core.Navigation.Models {
    public class ContentMenuItemPartRecord : ContentPartRecord {
        public virtual ContentItemRecord ContentMenuItemRecord { get; set; }
    }
}