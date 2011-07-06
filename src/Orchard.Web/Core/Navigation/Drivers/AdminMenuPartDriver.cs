using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Navigation.Models;
using Orchard.Localization;
using Orchard.Security;

namespace Orchard.Core.Navigation.Drivers {
    public class AdminMenuPartDriver : ContentPartDriver<AdminMenuPart> {
        private readonly IAuthorizationService _authorizationService;
        private readonly IOrchardServices _orchardServices;

        public AdminMenuPartDriver(IAuthorizationService authorizationService, IOrchardServices orchardServices) {
            _authorizationService = authorizationService;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(AdminMenuPart part, dynamic shapeHelper) {
            // todo: we need a 'ManageAdminMenu' too?
            if (!_authorizationService.TryCheckAccess(Permissions.ManageMainMenu, _orchardServices.WorkContext.CurrentUser, part)) {
                return null;
            }

            return ContentShape("Parts_Navigation_AdminMenu_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Navigation.AdminMenu.Edit", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(AdminMenuPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!_authorizationService.TryCheckAccess(Permissions.ManageMainMenu, _orchardServices.WorkContext.CurrentUser, part))
                return null;

            updater.TryUpdateModel(part, Prefix, null, null);

            if (part.OnAdminMenu) {
                if (string.IsNullOrEmpty(part.AdminMenuText)) {
                    updater.AddModelError("AdminMenuText", T("The AdminMenuText field is required"));
                }
            }
            else {
                part.AdminMenuPosition = "";
            }

            return Editor(part, shapeHelper);
        }

        protected override void Importing(AdminMenuPart part, ContentManagement.Handlers.ImportContentContext context) {
            var adminMenuText = context.Attribute(part.PartDefinition.Name, "AdminMenuText");
            if (adminMenuText != null) {
                part.AdminMenuText = adminMenuText;
            }

            var position = context.Attribute(part.PartDefinition.Name, "AdminMenuPosition");
            if (position != null) {
                part.AdminMenuPosition = position;
            }

            var onAdminMenu = context.Attribute(part.PartDefinition.Name, "OnAdminMenu");
            if (onAdminMenu != null) {
                part.OnAdminMenu = Convert.ToBoolean(onAdminMenu);
            }
        }

        protected override void Exporting(AdminMenuPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("AdminMenuText", part.AdminMenuText);
            context.Element(part.PartDefinition.Name).SetAttributeValue("AdminMenuPosition", part.AdminMenuPosition);
            context.Element(part.PartDefinition.Name).SetAttributeValue("OnAdminMenu", part.OnAdminMenu);
        }
    }
}