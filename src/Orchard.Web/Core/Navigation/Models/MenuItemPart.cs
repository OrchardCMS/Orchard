using Orchard.ContentManagement;

namespace Orchard.Core.Navigation.Models {
    public class MenuItemPart : ContentPart {
        public string Url {
            get { return this.Retrieve(x => x.Url); }
            set { this.Store(x => x.Url, value); }
        }
    }
}
