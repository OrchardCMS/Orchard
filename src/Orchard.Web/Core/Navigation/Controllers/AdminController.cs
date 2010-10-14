using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Navigation.ViewModels;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.AntiForgery;
using Orchard.UI.Navigation;
using Orchard.Utility;

namespace Orchard.Core.Navigation.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IMenuService _menuService;
        private readonly IOrchardServices _services;
        private readonly INavigationManager _navigationManager;

        public AdminController(
            IMenuService menuService,
            IOrchardServices services,
            INavigationManager navigationManager,
            IShapeHelperFactory shapeHelperFactory) {
            _menuService = menuService;
            _services = services;
            _navigationManager = navigationManager;
            T = NullLocalizer.Instance;
            Shape = shapeHelperFactory.CreateHelper();
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(NavigationManagementViewModel model) {
            if (!_services.Authorizer.Authorize(Permissions.ManageMainMenu, T("Not allowed to manage the main menu")))
                return new HttpUnauthorizedResult();

            if (model == null)
                model = new NavigationManagementViewModel();

            if (model.MenuItemEntries == null || model.MenuItemEntries.Count() < 1)
                model.MenuItemEntries = _menuService.Get().Select(CreateMenuItemEntries).OrderBy(menuPartEntry => menuPartEntry.MenuItem.Position, new PositionComparer()).ToList();

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST(IList<MenuItemEntry> menuItemEntries) {
            if (!_services.Authorizer.Authorize(Permissions.ManageMainMenu, T("Couldn't manage the main menu")))
                return new HttpUnauthorizedResult();

            foreach (var menuItemEntry in menuItemEntries) {
                MenuPart menuPart = _menuService.Get(menuItemEntry.MenuItemId);

                menuPart.MenuText = menuItemEntry.MenuItem.Text;
                menuPart.MenuPosition = menuItemEntry.MenuItem.Position;
                if (menuPart.Is<MenuItemPart>())
                    menuPart.As<MenuItemPart>().Url = menuItemEntry.MenuItem.Url;
            }

            return RedirectToAction("Index");
        }

        private MenuItemEntry CreateMenuItemEntries(MenuPart menuPart) {
            return new MenuItemEntry {
                                         MenuItem = new MenuItem {
                                                                                   Text = menuPart.MenuText,
                                                                                   Position = menuPart.MenuPosition,
                                                                                   Url = menuPart.Is<MenuItemPart>()
                                                                                             ? menuPart.As<MenuItemPart>().Url
                                                                                             : _navigationManager.GetUrl(null, _services.ContentManager.GetItemMetadata(menuPart).DisplayRouteValues)
                                                                               },
                                         MenuItemId = menuPart.Id,
                                         IsMenuItem = menuPart.Is<MenuItemPart>()
                                     };
        }

        public ActionResult Create() {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Create(CreateMenuItemViewModel model) {
            if (!_services.Authorizer.Authorize(Permissions.ManageMainMenu, T("Couldn't manage the main menu")))
                return new HttpUnauthorizedResult();

            var menuPart = _services.ContentManager.New<MenuPart>("MenuItem");
            model.MenuItem = _services.ContentManager.UpdateEditor(menuPart, this);

            if (!ModelState.IsValid) {
                _services.TransactionManager.Cancel();
                return Index(new NavigationManagementViewModel {NewMenuItem = model});
            }

            if (string.IsNullOrEmpty(menuPart.MenuPosition))
                menuPart.MenuPosition = Position.GetNext(_navigationManager.BuildMenu("main"));
            menuPart.OnMainMenu = true;

            _services.ContentManager.Create(model.MenuItem);

            return RedirectToAction("Index");
        }

        [ValidateAntiForgeryTokenOrchard]
        public ActionResult Delete(int id) {
            if (!_services.Authorizer.Authorize(Permissions.ManageMainMenu, T("Couldn't manage the main menu")))
                return new HttpUnauthorizedResult();

            MenuPart menuPart = _menuService.Get(id);

            if (menuPart != null) {
                if (menuPart.Is<MenuItemPart>())
                    _menuService.Delete(menuPart);
                else
                    menuPart.OnMainMenu = false;
            }

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
