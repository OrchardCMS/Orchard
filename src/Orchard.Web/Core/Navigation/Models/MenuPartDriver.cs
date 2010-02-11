using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Security;

namespace Orchard.Core.Navigation.Models {
    [UsedImplicitly]
    public class MenuPartDriver : ContentPartDriver<MenuPart> {
        private readonly IAuthorizationService _authorizationService;

        public MenuPartDriver(IAuthorizationService authorizationService) {
            _authorizationService = authorizationService;
        }

        public virtual IUser CurrentUser { get; set; }

        protected override DriverResult Editor(MenuPart part) {
            if (!_authorizationService.TryCheckAccess(Permissions.ManageMainMenu, CurrentUser, part))
                return null;

            return ContentPartTemplate(part, "Parts/Navigation.EditMenuPart").Location("primary", "9");
        }

        protected override DriverResult Editor(MenuPart part, IUpdateModel updater) {
            if (!_authorizationService.TryCheckAccess(Permissions.ManageMainMenu, CurrentUser, part))
                return null;

            updater.TryUpdateModel(part, Prefix, null, null);
            return ContentPartTemplate(part, "Parts/Navigation.EditMenuPart").Location("primary", "9");
        }
    }
}
