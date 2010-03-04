using Orchard.ContentManagement.Records;

namespace Orchard.Core.Navigation.Models {
    public class MenuItemRecord : ContentPartRecord {
        public virtual string Url { get; set; }
    }
}