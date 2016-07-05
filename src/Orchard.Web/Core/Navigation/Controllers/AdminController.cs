using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;
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
using Orchard.Core.Navigation.Services;

namespace Orchard.Core.Navigation.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IMenuService _menuService;
        private readonly INavigationManager _navigationManager;
        private readonly IEnumerable<IContentHandler> _handlers;
        private readonly IMenuManager _menuManager;
        private readonly IOrchardServices _orchardServices;

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
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            var menus = GetMenus().ToList();
            var lastMenu = menus.OrderByDescending(x => x.Id).FirstOrDefault();
            var lastMenuId = lastMenu != null ? lastMenu.Id : default(int?);

            if (!menus.Any()) {
                if (!_orchardServices.Authorizer.Authorize(Permissions.ManageMenus, T("Not allowed to manage menus")))
                    return new HttpUnauthorizedResult();

                return RedirectToAction("Create", "Admin", new { area = "Contents", id = "Menu", returnUrl = Url.Action("EditLastMenu", new { id = lastMenuId }) });
            }

            var allowedMenus = menus.Where(menu => _orchardServices.Authorizer.Authorize(Permissions.ManageMenus, menu)).ToList();

            if (!allowedMenus.Any())
                return new HttpUnauthorizedResult();

            var viewModel = new NavigationIndexViewModel {
                Menus = allowedMenus.Select(x => new MenuEntry {
                    ContentItem = x,
                    MenuId = x.Id,
                    MenuName = _orchardServices.ContentManager.GetItemMetadata(x).DisplayText,
                }).ToList(),
                LastMenuId = lastMenuId
            };

            return View(viewModel);
        }

        [HttpPost]
        [ActionName("Index")]
        public ActionResult IndexPOST(NavigationIndexViewModel model) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageMenus, T("Not allowed to manage menus")))
                return new HttpUnauthorizedResult();

            switch (model.BulkAction) {
                case NavigationIndexBulkAction.Delete: {
                    if (model.Menus == null)
                        break;

                    var selectedMenuIds = model.Menus.Where(x => x.IsSelected).Select(x => x.MenuId).ToList();
                    var selectedMenus = _orchardServices.ContentManager.GetMany<ContentItem>(selectedMenuIds, VersionOptions.Latest, QueryHints.Empty);

                    foreach (var menu in selectedMenus)
                        _orchardServices.ContentManager.Remove(menu);

                    _orchardServices.Notifier.Information(T("The selected menus have been deleted."));
                }
                break;
            }

            return RedirectToAction("Index");
        }

        public ActionResult EditLastMenu(int? id) {
            var lastMenu = _orchardServices.ContentManager.Query("Menu").List().OrderByDescending(x => x.Id).FirstOrDefault();
            var lastMenuId = lastMenu != null ? lastMenu.Id : default(int?);

            if (lastMenuId == null && id == null)
                RedirectToAction("Index");

            return lastMenuId != id ? RedirectToAction("Edit", new { id = lastMenu.Id }) : RedirectToAction("Index");
        }

        public ActionResult Edit(int id) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageMenus, T("Not allowed to manage menus")))
                return new HttpUnauthorizedResult();

            var menu = GetMenu(id);

            if (menu == null)
                return RedirectToAction("Index");

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageMenus, menu))
                return new HttpUnauthorizedResult();

            var viewModel = new NavigationManagementViewModel {
                MenuItemEntries = _menuService.GetMenuParts(menu.Id).Select(CreateMenuItemEntries).OrderBy(menuPartEntry => menuPartEntry.Position, new FlatPositionComparer()).ToList(),
                MenuItemDescriptors = _menuManager.GetMenuItemTypes(),
                CurrentMenu = menu
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(IList<MenuItemEntry> menuItemEntries, int? menuId) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageMenus, T("Not allowed to manage menus")))
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

            return RedirectToAction("Edit", new { menuId });
        }

        private MenuItemEntry CreateMenuItemEntries(MenuPart menuPart) {
            return new MenuItemEntry {
                MenuItemId = menuPart.Id,
                IsMenuItem = menuPart.Is<MenuItemPart>(),
                Text = menuPart.MenuText,
                Position = menuPart.MenuPosition,
                Url = menuPart.Is<MenuItemPart>()
                              ? menuPart.As<MenuItemPart>().Url
                              : _navigationManager.GetUrl(null, _orchardServices.ContentManager.GetItemMetadata(menuPart).DisplayRouteValues),
                ContentItem = menuPart.ContentItem,
            };
        }

        [HttpPost]
        public ActionResult DeleteMenuItem(int id) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageMenus, T("Not allowed to manage menus")))
                return new HttpUnauthorizedResult();

            var menuPart = _menuService.Get(id);
            var menuId = default(int?);

            if (menuPart != null) {
                menuId = menuPart.Menu.Id;

                // get all sub-menu items from the same menu
                var menuItems = _menuService.GetMenuParts(menuPart.Menu.Id)
                    .Where(x => x.MenuPosition.StartsWith(menuPart.MenuPosition + "."))
                    .Select(x => x.As<MenuPart>())
                    .ToList();

                foreach (var menuItem in menuItems.Concat(new[] { menuPart })) {
                    // if the menu item is a concrete content item, don't delete it, just unreference the menu
                    if (!menuPart.ContentItem.TypeDefinition.Settings.ContainsKey("Stereotype") || menuPart.ContentItem.TypeDefinition.Settings["Stereotype"] != "MenuItem") {
                        menuPart.Menu = null;
                    }
                    else {
                        _menuService.Delete(menuItem);
                    }
                }

                _orchardServices.Notifier.Information(T("That menu item has been deleted."));
            }

            return menuId != null ? RedirectToAction("Edit", new { id = menuId }) : RedirectToAction("Index");
        }

        public ActionResult CreateMenuItem(string id, int menuId, string returnUrl) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageMenus, _menuService.GetMenu(menuId), T("Not allowed to manage menus")))
                return new HttpUnauthorizedResult();

            // create a new temporary menu item
            var menuPart = _orchardServices.ContentManager.New<MenuPart>(id);

            if (menuPart == null)
                return HttpNotFound();

            // load the menu
            var menu = _orchardServices.ContentManager.Get(menuId);

            if (menu == null)
                return HttpNotFound();

            try {
                // filter the content items for this specific menu
                menuPart.MenuPosition = Position.GetNext(_navigationManager.BuildMenu(menu));

                var model = _orchardServices.ContentManager.BuildEditor(menuPart);

                return View(model);
            }
            catch (Exception exception) {
                if (exception.IsFatal()) {
                    throw;
                }

                Logger.Error(T("Creating menu item failed: {0}", exception.Message).Text);
                _orchardServices.Notifier.Error(T("Creating menu item failed: {0}", exception.Message));
                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            }
        }

        [HttpPost, ActionName("CreateMenuItem")]
        public ActionResult CreateMenuItemPost(string id, int menuId, string returnUrl) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageMenus, _menuService.GetMenu(menuId), T("Not allowed to manage menus")))
                return new HttpUnauthorizedResult();

            var menuPart = _orchardServices.ContentManager.New<MenuPart>(id);

            if (menuPart == null)
                return HttpNotFound();

            // load the menu
            var menu = _orchardServices.ContentManager.Get(menuId);

            if (menu == null)
                return HttpNotFound();

            var model = _orchardServices.ContentManager.UpdateEditor(menuPart, this);

            menuPart.MenuPosition = Position.GetNext(_navigationManager.BuildMenu(menu));
            menuPart.Menu = menu;

            _orchardServices.ContentManager.Create(menuPart);

            if (!ModelState.IsValid) {
                _orchardServices.TransactionManager.Cancel();
                return View(model);
            }

            _orchardServices.Notifier.Success(T("Your {0} has been added.", menuPart.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        private ContentItem GetMenu(int id) {
            return _orchardServices.ContentManager.Get(id, VersionOptions.Latest);
        }

        private IEnumerable<ContentItem> GetMenus() {
            return _orchardServices.ContentManager.Query("Menu").List().ToList().OrderBy(x => x.ContentManager.GetItemMetadata(x).DisplayText);
        }

        private IEnumerable<ContentItem> GetAllowedMenus(IEnumerable<ContentItem> menus) {
            return menus.Where(menu => _orchardServices.Authorizer.Authorize(Permissions.ManageMenus, menu)).ToList();
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
