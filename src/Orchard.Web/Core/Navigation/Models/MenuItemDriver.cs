using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Orchard.Core.Navigation.Models {
    [UsedImplicitly]
    public class MenuItemDriver : ContentItemDriver<MenuItem> {
        private readonly IOrchardServices _orchardServices;

        public readonly static ContentType ContentType = new ContentType {
            Name = "menuitem",
            DisplayName = "Menu Item"
        };

        public MenuItemDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        protected override ContentType GetContentType() {
            return ContentType;
        }

        protected override string Prefix { get { return ""; } }

        protected override string GetDisplayText(MenuItem item) {
            return item.Url;
        }
    }
}
