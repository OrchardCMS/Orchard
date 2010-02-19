using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Navigation.Models;
using Orchard.Security;

namespace Orchard.Core.Navigation.Drivers {
    [UsedImplicitly]
    public class MenuItemDriver : ContentItemDriver<MenuItem> {
        private readonly IAuthorizationService _authorizationService;

        public readonly static ContentType ContentType = new ContentType {
                                                                             Name = "menuitem",
                                                                             DisplayName = "Menu Item"
                                                                         };

        public MenuItemDriver(IAuthorizationService authorizationService) {
            _authorizationService = authorizationService;
        }

        public virtual IUser CurrentUser { get; set; }

        protected override ContentType GetContentType() {
            return ContentType;
        }

        protected override string Prefix { get { return ""; } }

        protected override string GetDisplayText(MenuItem item) {
            return item.Url;
        }

        protected override DriverResult Editor(MenuItem item, IUpdateModel updater) {
            if (!_authorizationService.TryCheckAccess(Permissions.ManageMainMenu, CurrentUser, item))
                return null;

            updater.TryUpdateModel(item, Prefix, null, null);

            return null;
        }
    }
}