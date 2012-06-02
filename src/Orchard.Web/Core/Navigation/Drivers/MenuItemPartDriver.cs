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

        protected override DriverResult Editor(MenuItemPart part, dynamic shapeHelper) {
            var currentUser = _workContextAccessor.GetContext().CurrentUser;
            if (!_authorizationService.TryCheckAccess(Permissions.ManageMainMenu, currentUser, part))
                return null;

            return ContentShape("Parts_MenuItem_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.MenuItem.Edit", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(MenuItemPart part, IUpdateModel updater, dynamic shapeHelper) {
            var currentUser = _workContextAccessor.GetContext().CurrentUser;
            if (!_authorizationService.TryCheckAccess(Permissions.ManageMainMenu, currentUser, part))
                return null;

            if (updater != null) {
                updater.TryUpdateModel(part, Prefix, null, null);
            }

            return Editor(part, shapeHelper);
        }

        protected override void Importing(MenuItemPart part, ContentManagement.Handlers.ImportContentContext context) {
            var url = context.Attribute(part.PartDefinition.Name, "Url");
            if (url != null) {
                part.Url = url;
            }
        }

        protected override void Exporting(MenuItemPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Url", part.Url);
        }
    }
}