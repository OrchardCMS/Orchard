﻿using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Navigation.ViewModels;
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
        private readonly IMenuService _menuService;

        public MenuPartDriver(
            IAuthorizationService authorizationService, 
            INavigationManager navigationManager, 
            IOrchardServices orchardServices,
            IMenuService menuService) {
            _authorizationService = authorizationService;
            _navigationManager = navigationManager;
            _orchardServices = orchardServices;
            _menuService = menuService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix {
            get {
                return "MenuPart";
            }
        }

        protected override DriverResult Editor(MenuPart part, dynamic shapeHelper) {
            var allowedMenus = _menuService.GetMenus().Where(menu => _authorizationService.TryCheckAccess(Permissions.ManageMenus, _orchardServices.WorkContext.CurrentUser, menu)).ToList();

            if (!allowedMenus.Any())
                return null;

            return ContentShape("Parts_Navigation_Menu_Edit", () => {
                var model = new MenuPartViewModel {
                    CurrentMenuId = part.Menu == null ? -1 : part.Menu.Id,
                    ContentItem = part.ContentItem,
                    Menus = allowedMenus,
                    OnMenu = part.Menu != null,
                    MenuText = part.MenuText
                };

                return shapeHelper.EditorTemplate(TemplateName: "Parts.Navigation.Menu.Edit", Model: model, Prefix: Prefix);
            });
        }

        protected override DriverResult Editor(MenuPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new MenuPartViewModel();

            if(updater.TryUpdateModel(model, Prefix, null, null)) {
                var menu = model.OnMenu ? _orchardServices.ContentManager.Get(model.CurrentMenuId) : null;

                if (!_authorizationService.TryCheckAccess(Permissions.ManageMenus, _orchardServices.WorkContext.CurrentUser, menu))
                    return null;

                part.MenuText = model.MenuText;
                part.Menu = menu;

                if (string.IsNullOrEmpty(part.MenuPosition) && menu != null) {
                    part.MenuPosition = Position.GetNext(_navigationManager.BuildMenu(menu));

                    if (string.IsNullOrEmpty(part.MenuText)) {
                        updater.AddModelError("MenuText", T("The MenuText field is required"));
                    }
                }
            }

            return Editor(part, shapeHelper);
        }

        protected override void Importing(MenuPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "MenuText", menuText =>
                part.MenuText = menuText
            );

            context.ImportAttribute(part.PartDefinition.Name, "MenuPosition", position =>
                part.MenuPosition = position
            );

            context.ImportAttribute(part.PartDefinition.Name, "Menu", menuIdentity => {
                var menu = context.GetItemFromSession(menuIdentity);
                if (menu != null) {
                    part.Menu = menu;
                }
            });
        }

        protected override void Exporting(MenuPart part, ContentManagement.Handlers.ExportContentContext context) {
            // is it on a menu ?
            if(part.Menu == null) {
                return;
            }

            var menu = _orchardServices.ContentManager.Get(part.Menu.Id);
            var menuIdentity = _orchardServices.ContentManager.GetItemMetadata(menu).Identity;
            context.Element(part.PartDefinition.Name).SetAttributeValue("Menu", menuIdentity);

            context.Element(part.PartDefinition.Name).SetAttributeValue("MenuText", part.MenuText);
            context.Element(part.PartDefinition.Name).SetAttributeValue("MenuPosition", part.MenuPosition);
        }
    }
}