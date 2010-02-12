using Orchard.ContentManagement.Records;

namespace Orchard.Core.Navigation.Records {
    public class MenuItemRecord : ContentPartRecord {
        public virtual string Url { get; set; }
    }
}