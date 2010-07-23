using Orchard.ContentManagement.Records;

namespace Orchard.Core.Navigation.Models {
    public class MenuItemPartRecord : ContentPartRecord {
        public virtual string Url { get; set; }
    }
}