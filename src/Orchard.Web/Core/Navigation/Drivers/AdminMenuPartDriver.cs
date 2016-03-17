using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Settings;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;
using Orchard.Utility;

namespace Orchard.Core.Navigation.Drivers {
    public class AdminMenuPartDriver : ContentPartDriver<AdminMenuPart> {
        private readonly IAuthorizationService _authorizationService;
        private readonly INavigationManager _navigationManager;
        private readonly IOrchardServices _orchardServices;

        public AdminMenuPartDriver(IAuthorizationService authorizationService, INavigationManager navigationManager, IOrchardServices orchardServices) {
            _authorizationService = authorizationService;
            _navigationManager = navigationManager;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private string GetDefaultPosition(ContentPart part) {
            var settings = part.Settings.GetModel<AdminMenuPartTypeSettings>();
            var defaultPosition = settings == null ? "" : settings.DefaultPosition;
            var adminMenu = _navigationManager.BuildMenu("admin");
            if (!string.IsNullOrEmpty(defaultPosition)) {
                int major;
                return int.TryParse(defaultPosition, out major) ? Position.GetNextMinor(major, adminMenu) : defaultPosition;
            }
            return Position.GetNext(adminMenu);
        }

        protected override DriverResult Editor(AdminMenuPart part, dynamic shapeHelper) {
            // todo: we need a 'ManageAdminMenu' too?
            if (!_authorizationService.TryCheckAccess(Permissions.ManageMenus, _orchardServices.WorkContext.CurrentUser, part)) {
                return null;
            }

            if (string.IsNullOrEmpty(part.AdminMenuPosition)) {
                part.AdminMenuPosition = GetDefaultPosition(part);
            }

            return ContentShape("Parts_Navigation_AdminMenu_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Navigation.AdminMenu.Edit", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(AdminMenuPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!_authorizationService.TryCheckAccess(Permissions.ManageMenus, _orchardServices.WorkContext.CurrentUser, part))
                return null;

            updater.TryUpdateModel(part, Prefix, null, null);

            if (part.OnAdminMenu) {
                if (string.IsNullOrEmpty(part.AdminMenuText)) {
                    updater.AddModelError("AdminMenuText", T("The AdminMenuText field is required"));
                }

                if (string.IsNullOrEmpty(part.AdminMenuPosition)) {
                    part.AdminMenuPosition = GetDefaultPosition(part);
                }
            }
            else {
                part.AdminMenuPosition = "";
            }

            return Editor(part, shapeHelper);
        }

        protected override void Importing(AdminMenuPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "AdminMenuText", adminMenuText =>
                part.AdminMenuText = adminMenuText
            );

            context.ImportAttribute(part.PartDefinition.Name, "AdminMenuPosition", position =>
                part.AdminMenuPosition = position
            );

            context.ImportAttribute(part.PartDefinition.Name, "OnAdminMenu", onAdminMenu =>
                part.OnAdminMenu = Convert.ToBoolean(onAdminMenu)
            );
        }

        protected override void Exporting(AdminMenuPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("AdminMenuText", part.AdminMenuText);
            context.Element(part.PartDefinition.Name).SetAttributeValue("AdminMenuPosition", part.AdminMenuPosition);
            context.Element(part.PartDefinition.Name).SetAttributeValue("OnAdminMenu", part.OnAdminMenu);
        }
    }
}