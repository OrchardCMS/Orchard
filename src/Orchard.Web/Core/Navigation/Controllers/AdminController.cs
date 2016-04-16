using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Navigation.ViewModels;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.UI;
using Orchard.UI.Notify;
using Orchard.UI.Navigation;
using Orchard.Utility;
using System;
using Orchard.ContentManagement.Handlers;
using Orchard.Logging;
using Orchard.Exceptions;

namespace Orchard.Core.Navigation.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IMenuService _menuService;
        private readonly INavigationManager _navigationManager;
        private readonly IEnumerable<IContentHandler> _handlers;
        private readonly IMenuManager _menuManager;

        public AdminController(
            IOrchardServices orchardServices,
            IMenuService menuService,
            IMenuManager menuManager,
            INavigationManager navigationManager,
            IEnumerable<IContentHandler> handlers) {
            _menuService = menuService;
            _menuManager = menuManager;
            _navigationManager = navigationManager;
            _handlers = handlers;

            Services = orchardServices;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        public IOrchardServices Services { get; set; }

        public ActionResult Index(NavigationManagementViewModel model, int? menuId) {
            var menus = Services.ContentManager.Query("Menu").List().ToList()
                .OrderBy(x => x.ContentManager.GetItemMetadata(x).DisplayText);

            if (!menus.Any()) {
                if (!Services.Authorizer.Authorize(Permissions.ManageMenus, T("Not allowed to manage menus"))) {
                    return new HttpUnauthorizedResult();
                }

                return RedirectToAction("Create", "Admin", new { area = "Contents", id = "Menu", returnUrl = Request.RawUrl });
            }

            var allowedMenus = menus.Where(menu => Services.Authorizer.Authorize(Permissions.ManageMenus, menu)).ToList();

            if (!allowedMenus.Any()) {
                return new HttpUnauthorizedResult();
            }

            IContent currentMenu = menuId == null
                ? allowedMenus.FirstOrDefault()
                : allowedMenus.FirstOrDefault(menu => menu.Id == menuId);

            if (currentMenu == null && menuId != null) { // incorrect menu id passed
                return RedirectToAction("Index");
            }

            if (model == null) {
                model = new NavigationManagementViewModel();
            }

            if (model.MenuItemEntries == null || !model.MenuItemEntries.Any()) {
                model.MenuItemEntries = _menuService.GetMenuParts(currentMenu.Id).Select(CreateMenuItemEntries).OrderBy(menuPartEntry => menuPartEntry.Position, new FlatPositionComparer()).ToList();
            }

            model.MenuItemDescriptors = _menuManager.GetMenuItemTypes();
            model.Menus = allowedMenus;
            model.CurrentMenu = currentMenu;

            // need action name as this action is referenced from another action
            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(IList<MenuItemEntry> menuItemEntries, int? menuId) {
            if (!Services.Authorizer.Authorize(Permissions.ManageMenus, T("Couldn't manage the main menu")))
                return new HttpUnauthorizedResult();

            // See https://github.com/OrchardCMS/Orchard/issues/948
            if (menuItemEntries != null) {
                foreach (var menuItemEntry in menuItemEntries) {
                    MenuPart menuPart = _menuService.Get(menuItemEntry.MenuItemId);

                    if (menuPart.MenuPosition != menuItemEntry.Position) {
                        var context = new UpdateContentContext(menuPart.ContentItem);

                        _handlers.Invoke(handler => handler.Updating(context), Logger);

                        menuPart.MenuPosition = menuItemEntry.Position;

                        _handlers.Invoke(handler => handler.Updated(context), Logger);
                    }
                }
            }

            return RedirectToAction("Index", new { menuId });
        }

        private MenuItemEntry CreateMenuItemEntries(MenuPart menuPart) {
            return new MenuItemEntry {
                MenuItemId = menuPart.Id,
                IsMenuItem = menuPart.Is<MenuItemPart>(),
                Text = menuPart.MenuText,
                Position = menuPart.MenuPosition,
                Url = menuPart.Is<MenuItemPart>()
                              ? menuPart.As<MenuItemPart>().Url
                              : _navigationManager.GetUrl(null, Services.ContentManager.GetItemMetadata(menuPart).DisplayRouteValues),
                ContentItem = menuPart.ContentItem,
            };
        }

        [HttpPost]
        public ActionResult Delete(int id) {
            if (!Services.Authorizer.Authorize(Permissions.ManageMenus, T("Couldn't manage the main menu")))
                return new HttpUnauthorizedResult();

            MenuPart menuPart = _menuService.Get(id);
            int? menuId = null;

            if (menuPart != null) {
                menuId = menuPart.Menu.Id;

                // get all sub-menu items from the same menu
                var menuItems = _menuService.GetMenuParts(menuPart.Menu.Id)
                    .Where(x => x.MenuPosition.StartsWith(menuPart.MenuPosition + "."))
                    .Select(x => x.As<MenuPart>())
                    .ToList();

                foreach (var menuItem in menuItems.Concat(new [] {menuPart})) {
                    // if the menu item is a concrete content item, don't delete it, just unreference the menu
                    if (!menuPart.ContentItem.TypeDefinition.Settings.ContainsKey("Stereotype") || menuPart.ContentItem.TypeDefinition.Settings["Stereotype"] != "MenuItem") {
                        menuPart.Menu = null;
                    }
                    else {
                        _menuService.Delete(menuItem);
                    }
                }

            }

            return RedirectToAction("Index", new { menuId });
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public ActionResult CreateMenuItem(string id, int menuId, string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.ManageMenus, _menuService.GetMenu(menuId), T("Couldn't manage the main menu")))
                return new HttpUnauthorizedResult();

            // create a new temporary menu item
            var menuPart = Services.ContentManager.New<MenuPart>(id);

            if (menuPart == null)
                return HttpNotFound();
            
            // load the menu
            var menu = Services.ContentManager.Get(menuId);

            if (menu == null)
                return HttpNotFound();
            
            try {
                // filter the content items for this specific menu
                menuPart.MenuPosition = Position.GetNext(_navigationManager.BuildMenu(menu));
                
                var model = Services.ContentManager.BuildEditor(menuPart);
                
                return View(model);
            }
            catch (Exception exception) {
                if (exception.IsFatal()) {
                    throw;
                } 

                Logger.Error(T("Creating menu item failed: {0}", exception.Message).Text);
                Services.Notifier.Error(T("Creating menu item failed: {0}", exception.Message));
                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            }
        }

        [HttpPost, ActionName("CreateMenuItem")]
        public ActionResult CreateMenuItemPost(string id, int menuId, string returnUrl) {
            if (!Services.Authorizer.Authorize(Permissions.ManageMenus, _menuService.GetMenu(menuId), T("Couldn't manage the main menu")))
                return new HttpUnauthorizedResult();

            var menuPart = Services.ContentManager.New<MenuPart>(id);

            if (menuPart == null)
                return HttpNotFound();

            // load the menu
            var menu = Services.ContentManager.Get(menuId);

            if (menu == null)
                return HttpNotFound();
            
            var model = Services.ContentManager.UpdateEditor(menuPart, this);

            menuPart.MenuPosition = Position.GetNext(_navigationManager.BuildMenu(menu));
            menuPart.Menu = menu;

            Services.ContentManager.Create(menuPart);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            Services.Notifier.Success(T("Your {0} has been added.", menuPart.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }
    }
}
