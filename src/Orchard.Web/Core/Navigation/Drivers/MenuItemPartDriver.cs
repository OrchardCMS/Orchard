using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Navigation.Models;
using Orchard.Security;

namespace Orchard.Core.Navigation.Drivers {
    [UsedImplicitly]
    public class MenuItemPartDriver : ContentItemDriver<MenuItemPart> {
        private readonly IAuthorizationService _authorizationService;

        public readonly static ContentType ContentType = new ContentType {
                                                                             Name = "MenuItem",
                                                                             DisplayName = "Menu Item"
                                                                         };

        public MenuItemPartDriver(IAuthorizationService authorizationService) {
            _authorizationService = authorizationService;
        }

        public virtual IUser CurrentUser { get; set; }

        protected override ContentType GetContentType() {
            return ContentType;
        }

        protected override string Prefix { get { return ""; } }

        protected override string GetDisplayText(MenuItemPart itemPart) {
            return itemPart.Url;
        }

        protected override DriverResult Editor(MenuItemPart itemPart, IUpdateModel updater) {
            if (!_authorizationService.TryCheckAccess(Permissions.ManageMainMenu, CurrentUser, itemPart))
                return null;

            updater.TryUpdateModel(itemPart, Prefix, null, null);

            return null;
        }
    }
}