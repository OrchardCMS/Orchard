using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Navigation.Models;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;
using Orchard.Utility;

namespace Orchard.Core.Navigation.Drivers {
    [UsedImplicitly]
    public class MenuPartDriver : ContentPartDriver<MenuPart> {
        private readonly IAuthorizationService _authorizationService;
        private readonly INavigationManager _navigationManager;
        private readonly IOrchardServices _orchardServices;

        public MenuPartDriver(IAuthorizationService authorizationService, INavigationManager navigationManager, IOrchardServices orchardServices) {
            _authorizationService = authorizationService;
            _navigationManager = navigationManager;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(MenuPart part, dynamic shapeHelper) {
            if (!_authorizationService.TryCheckAccess(Permissions.ManageMainMenu, _orchardServices.WorkContext.CurrentUser, part))
                return null;

            return ContentShape("Parts_Navigation_Menu_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/Navigation.Menu.Edit", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(MenuPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!_authorizationService.TryCheckAccess(Permissions.ManageMainMenu, _orchardServices.WorkContext.CurrentUser, part))
                return null;

            if (string.IsNullOrEmpty(part.MenuPosition))
                part.MenuPosition = Position.GetNext(_navigationManager.BuildMenu("main"));

            updater.TryUpdateModel(part, Prefix, null, null);

            if (part.OnMainMenu && string.IsNullOrEmpty(part.MenuText))
                updater.AddModelError("MenuText", T("The MenuText field is required"));

            return Editor(part, shapeHelper);
        }
    }
}