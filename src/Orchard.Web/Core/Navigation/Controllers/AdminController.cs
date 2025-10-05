using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Contents.Settings;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Navigation.ViewModels;
using Orchard.Data;
using Orchard.Exceptions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;
using Orchard.Security;
using Orchard.UI;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Utility;
using Orchard.Utility.Extensions;

namespace Orchard.Core.Navigation.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IContentManager _contentManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;
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
            _contentManager = orchardServices.ContentManager;
            _transactionManager = orchardServices.TransactionManager;
            _authorizer = orchardServices.Authorizer;
            _notifier = orchardServices.Notifier;
            _menuService = menuService;
            _menuManager = menuManager;
            _navigationManager = navigationManager;
            _handlers = handlers;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index(NavigationManagementViewModel model, int? menuId) {
            var menus = _contentManager.Query("Menu").List().ToList()
                .OrderBy(x => x.ContentManager.GetItemMetadata(x).DisplayText);

            if (!menus.Any()) {
                if (!_authorizer.Authorize(Permissions.ManageMenus, T("Not allowed to manage menus"))) {
                    return new HttpUnauthorizedResult();
                }

                return RedirectToAction("Create", "Admin", new { area = "Contents", id = "Menu", returnUrl = Request.RawUrl });
            }

            var allowedMenus = menus.Where(menu => _authorizer.Authorize(Permissions.ManageMenus, menu)).ToList();

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
                model.MenuItemEntries = _menuService
                    .GetMenuParts(currentMenu.Id)
                    .Select(CreateMenuItemEntries)
                    .OrderBy(menuPartEntry => menuPartEntry.Position, new FlatPositionComparer())
                    .ToList();
            }

            model.MenuItemDescriptors = _menuManager.GetMenuItemTypes();
            model.Menus = allowedMenus;
            model.CurrentMenu = currentMenu;

            // need action name as this action is referenced from another action
            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(IList<MenuItemEntry> menuItemEntries, int? menuId) {
            if (!_authorizer.Authorize(
                Permissions.ManageMenus,
                menuId.HasValue ? _menuService.GetMenu(menuId.Value) : null,
                T("Couldn't manage the menu")))
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

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Delete")]
        public ActionResult EditDeletePOST(int id) => Delete(id);

        [HttpPost]
        public ActionResult Delete(int id) {
            MenuPart menuPart = _menuService.Get(id);
            int? menuId = null;

            if (!_authorizer.Authorize(
                Permissions.ManageMenus,
                menuPart == null ? null : _menuService.GetMenu(menuPart.Menu.Id),
                T("Couldn't manage the menu")))
                return new HttpUnauthorizedResult();

            if (menuPart != null) {
                menuId = menuPart.Menu.Id;

                // get all sub-menu items from the same menu
                var menuItems = _menuService.GetMenuParts(menuPart.Menu.Id)
                    .Where(x => x.MenuPosition.StartsWith(menuPart.MenuPosition + "."))
                    .Select(x => x.As<MenuPart>())
                    .ToList();

                foreach (var menuItem in menuItems.Concat(new[] { menuPart })) {
                    // if the menu item is a concrete content item, don't delete it, just remove the menu reference
                    if (!menuPart.ContentItem.TypeDefinition.Settings.ContainsKey("Stereotype")
                        || menuPart.ContentItem.TypeDefinition.Settings["Stereotype"] != "MenuItem") {
                        menuPart.Menu = null;
                    }
                    else {
                        _menuService.Delete(menuItem);
                    }
                }

                _notifier.Information(T.Plural(
                    "The menu item '{1}' has been deleted.",
                    "The menu item '{1}' and its children have been deleted.",
                    menuItems.Count() + 1,
                    menuPart.MenuText));
            }

            return RedirectToAction("Index", new { menuId });
        }

        public ActionResult CreateMenuItem(string id, int menuId, string returnUrl) {
            if (!_authorizer.Authorize(Permissions.ManageMenus, _menuService.GetMenu(menuId), T("Couldn't manage the menu")))
                return new HttpUnauthorizedResult();

            // create a new temporary menu item
            var contentItem = _contentManager.New(id);
            var menuPart = contentItem.As<MenuPart>();

            if (menuPart == null)
                return HttpNotFound();

            // load the menu
            var menu = _contentManager.Get(menuId);

            if (menu == null)
                return HttpNotFound();

            try {
                // filter the content items for this specific menu
                menuPart.MenuPosition = Position.GetNext(_navigationManager.BuildMenu(menu));
                menuPart.Menu = menu;
                var model = _contentManager.BuildEditor(contentItem);

                return View(model);
            }
            catch (Exception exception) {
                if (exception.IsFatal()) {
                    throw;
                }

                Logger.Error(T("Creating menu item failed: {0}", exception.Message).Text);
                _notifier.Error(T("Creating menu item failed: {0}", exception.Message));
                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            }
        }

        [HttpPost, ActionName("CreateMenuItem")]
        public ActionResult CreateMenuItemPost(string id, int menuId, string returnUrl) {
            if (!_authorizer.Authorize(Permissions.ManageMenus, _menuService.GetMenu(menuId), T("Couldn't manage the menu")))
                return new HttpUnauthorizedResult();

            var contentItem = _contentManager.New(id);
            var menuPart = contentItem.As<MenuPart>();

            if (menuPart == null)
                return HttpNotFound();

            // load the menu
            var menu = _contentManager.Get(menuId);
            if (menu == null)
                return HttpNotFound();

            _contentManager.Create(contentItem);

            menuPart.Menu = menu;
            menuPart.MenuPosition = Position.GetNext(_navigationManager.BuildMenu(menu));

            var model = _contentManager.UpdateEditor(contentItem, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(model);
            }

            _notifier.Information(T("Your {0} has been added.", contentItem.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        public ActionResult Edit(int id) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (contentItem == null)
                return HttpNotFound();

            if (!_authorizer.Authorize(Permissions.ManageMenus, contentItem.Content.MenuPart.Menu, T("Couldn't manage the menu")))
                return new HttpUnauthorizedResult();

            var model = _contentManager.BuildEditor(contentItem);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [Mvc.FormValueRequired("submit.Publish")]
        public ActionResult EditPOST(int id, string returnUrl) {
            return EditPOST(id, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>()
                    && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable
                    && contentItem.IsPublished())
                    _contentManager.Publish(contentItem);
            });
        }

        [HttpPost]
        // Copy of Contents/AdminController/Publish, but with different permission check and redirect.
        public ActionResult Publish(int id) {
            var menuPart = _contentManager.GetLatest<MenuPart>(id);
            if (menuPart == null)
                return HttpNotFound();

            if (!_authorizer.Authorize(Permissions.ManageMenus, menuPart.Menu, T("Couldn't manage the menu")))
                return new HttpUnauthorizedResult();

            _contentManager.Publish(menuPart.ContentItem);

            _notifier.Success(
                string.IsNullOrWhiteSpace(menuPart.MenuText)
                    ? string.IsNullOrWhiteSpace(menuPart.TypeDefinition.DisplayName)
                        ? T("Your content has been published.")
                        : T("Your {0} has been published.", menuPart.TypeDefinition.DisplayName)
                    : T("'{0}' has been published.", menuPart.MenuText));

            return RedirectToAction("Index", new { menuId = menuPart.Menu.Id });
        }

        [HttpPost]
        // Copy of Contents/AdminController/Unpublish, but with different permission check and redirect.
        public ActionResult Unpublish(int id) {
            var menuPart = _contentManager.GetLatest<MenuPart>(id);
            if (menuPart == null)
                return HttpNotFound();

            if (!_authorizer.Authorize(Permissions.ManageMenus, menuPart.Menu, T("Couldn't manage the menu")))
                return new HttpUnauthorizedResult();

            _contentManager.Unpublish(menuPart.ContentItem);

            _notifier.Success(
                string.IsNullOrWhiteSpace(menuPart.MenuText)
                    ? string.IsNullOrWhiteSpace(menuPart.TypeDefinition.DisplayName)
                        ? T("Your content has been unpublished.")
                        : T("Your {0} has been unpublished.", menuPart.TypeDefinition.DisplayName)
                    : T("'{0}' has been unpublished.", menuPart.MenuText));

            return RedirectToAction("Index", new { menuId = menuPart.Menu.Id });
        }

        [HttpPost, ActionName("Edit")]
        [Mvc.FormValueRequired("submit.Unpublish")]
        public ActionResult EditUnpublishPOST(int id) => Unpublish(id);

        private MenuItemEntry CreateMenuItemEntries(MenuPart menuPart) {
            return new MenuItemEntry {
                MenuItemId = menuPart.Id,
                IsMenuItem = menuPart.Is<MenuItemPart>(),
                Text = menuPart.MenuText,
                Position = menuPart.MenuPosition,
                Url = menuPart.Is<MenuItemPart>()
                    ? menuPart.As<MenuItemPart>().Url
                    : _navigationManager.GetUrl(null, _contentManager.GetItemMetadata(menuPart).DisplayRouteValues),
                ContentItem = menuPart.ContentItem,
            };
        }

        private ActionResult EditPOST(int id, string returnUrl, Action<ContentItem> conditionallyPublish) {
            var contentItem = _contentManager.GetLatest(id);
            var menuPart = contentItem.As<MenuPart>();

            if (menuPart == null)
                return HttpNotFound();

            if (!_authorizer.Authorize(Permissions.ManageMenus, menuPart.Menu, T("Couldn't manage the menu")))
                return new HttpUnauthorizedResult();

            string previousRoute = null;
            if (contentItem.Has<IAliasAspect>()
                && !string.IsNullOrWhiteSpace(returnUrl)
                && Request.IsLocalUrl(returnUrl)
                // only if the original returnUrl is the content itself
                && String.Equals(returnUrl, Url.ItemDisplayUrl(contentItem), StringComparison.OrdinalIgnoreCase)
                ) {
                previousRoute = contentItem.As<IAliasAspect>().Path;
            }

            var model = _contentManager.UpdateEditor(contentItem, this);
            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View("Edit", model);
            }

            conditionallyPublish(contentItem);

            if (!string.IsNullOrWhiteSpace(returnUrl)
                && previousRoute != null
                && !String.Equals(contentItem.As<IAliasAspect>().Path, previousRoute, StringComparison.OrdinalIgnoreCase)) {
                returnUrl = Url.ItemDisplayUrl(contentItem);
            }

            _notifier.Success(
                string.IsNullOrWhiteSpace(menuPart.MenuText)
                    ? string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                        ? T("Your content has been saved.")
                        : T("Your {0} has been saved.", contentItem.TypeDefinition.DisplayName)
                    : T("'{0}' has been saved.", menuPart.MenuText));

            return RedirectToAction("Index", new { menuId = menuPart.Menu.Id });
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
