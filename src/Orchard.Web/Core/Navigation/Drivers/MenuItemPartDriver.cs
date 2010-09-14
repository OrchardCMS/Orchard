using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Navigation.Models;
using Orchard.Security;

namespace Orchard.Core.Navigation.Drivers {
    [UsedImplicitly]
    public class MenuItemPartDriver : ContentPartDriver<MenuItemPart> {
        private readonly IAuthorizationService _authorizationService;
        private readonly IWorkContextAccessor _workContextAccessor;

        public MenuItemPartDriver(IAuthorizationService authorizationService, IWorkContextAccessor workContextAccessor) {
            _authorizationService = authorizationService;
            _workContextAccessor = workContextAccessor;
        }

        protected override DriverResult Editor(MenuItemPart itemPart, IUpdateModel updater) {
            //todo: (heskew) need context
            var currentUser = _workContextAccessor.GetContext().CurrentUser;

            if (!_authorizationService.TryCheckAccess(Permissions.ManageMainMenu, currentUser, itemPart))
                return null;

            updater.TryUpdateModel(itemPart, Prefix, null, null);

            return null;
        }
    }
}