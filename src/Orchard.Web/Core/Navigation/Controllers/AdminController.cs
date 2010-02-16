using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Drivers;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Navigation.ViewModels;
using Orchard.Localization;
using Orchard.UI.Navigation;
using Orchard.Utility;
using MenuItem=Orchard.Core.Navigation.Models.MenuItem;

namespace Orchard.Core.Navigation.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IMenuService _menuService;
        private readonly IOrchardServices _services;
        private readonly INavigationManager _navigationManager;

        public AdminController(IMenuService menuService, IOrchardServices services, INavigationManager navigationManager) {
            _menuService = menuService;
            _services = services;
            _navigationManager = navigationManager;
            T = NullLocalizer.Instance;
        }

        private Localizer T { get; set; }

        public ActionResult Index(NavigationManagementViewModel model) {
            if (!_services.Authorizer.Authorize(Permissions.ManageMainMenu, T("Not allowed to manage the main menu")))
                return new HttpUnauthorizedResult();

            if (model == null)
                model = new NavigationManagementViewModel();

            if (model.MenuItemEntries == null || model.MenuItemEntries.Count() < 1)
                model.MenuItemEntries = _menuService.Get().Select(menuPart => CreateMenuItemEntries(menuPart)).OrderBy(menuPartEntry => menuPartEntry.MenuItem.Position, new PositionComparer()).ToList();

            return View("Index", model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(IList<MenuItemEntry> menuItemEntries) {
            if (!_services.Authorizer.Authorize(Permissions.ManageMainMenu, T("Couldn't manage the main menu")))
                return new HttpUnauthorizedResult();

            foreach (var menuItemEntry in menuItemEntries) {
                MenuPart menuPart = _menuService.Get(menuItemEntry.MenuItemId);

                menuPart.MenuText = menuItemEntry.MenuItem.Text;
                menuPart.MenuPosition = menuItemEntry.MenuItem.Position;
                if (menuPart.Is<MenuItem>())
                    menuPart.As<MenuItem>().Url = menuItemEntry.MenuItem.Url;

                _services.ContentManager.UpdateEditorModel(menuPart, this);
            }

            return RedirectToAction("Index");
        }

        private static MenuItemEntry CreateMenuItemEntries(MenuPart menuPart) {
            return new MenuItemEntry {
                                         MenuItem = new UI.Navigation.MenuItem {
                                                                                   Text = menuPart.MenuText,
                                                                                   Position = menuPart.MenuPosition,
                                                                                   Url = menuPart.As<MenuItem>().Url
                                                                               },
                                         MenuItemId = menuPart.Id
                                     };
        }

        public ActionResult Create() {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Create(CreateMenuItemViewModel model) {
            if (!_services.Authorizer.Authorize(Permissions.ManageMainMenu, T("Couldn't manage the main menu")))
                return new HttpUnauthorizedResult();

            var menuPart = _services.ContentManager.New<MenuPart>(MenuItemDriver.ContentType.Name);
            model.MenuItem = _services.ContentManager.UpdateEditorModel(menuPart, this);

            if (!ModelState.IsValid) {
                _services.TransactionManager.Cancel();
                return Index(new NavigationManagementViewModel {NewMenuItem = model});
            }

            if (string.IsNullOrEmpty(menuPart.MenuPosition))
                menuPart.MenuPosition = Position.GetNext(_navigationManager.BuildMenu("main"));
            menuPart.OnMainMenu = true;

            _services.ContentManager.Create(model.MenuItem.Item.ContentItem);

            return RedirectToAction("Index");
        }

        //[ValidateAntiForgeryTokenOrchard, ActionName("Delete")]
        [HttpPost]
        public ActionResult Delete(int menuItemId)
        {
            if (!_services.Authorizer.Authorize(Permissions.ManageMainMenu, T("Couldn't manage the main menu")))
                return new HttpUnauthorizedResult();

            //todo -> delete

            return RedirectToAction("Index");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
