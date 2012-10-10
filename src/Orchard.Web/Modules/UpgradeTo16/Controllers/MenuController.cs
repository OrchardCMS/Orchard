using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;
using Orchard.Core.Navigation.Services;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard.Widgets.Models;
using Orchard.Widgets.Services;

namespace UpgradeTo16.Controllers {
    [Admin]
    public class MenuController : Controller {
        private readonly IMenuService _menuService;
        private readonly IOrchardServices _orchardServices;
        private readonly IWidgetsService _widgetsService;

        public MenuController(
            IMenuService menuService,
            IOrchardServices orchardServices,
            IWidgetsService widgetsService ) {
            _menuService = menuService;
            _orchardServices = orchardServices;
            _widgetsService = widgetsService;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            var menus = _menuService.GetMenus();

            if(menus.Any()) {
                _orchardServices.Notifier.Warning(T("This step is unnecessary as some menus already exist."));
            }

            return View();
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to migrate the navigation.")))
                return new HttpUnauthorizedResult();

            var menus = _menuService.GetMenus();

            if (menus.Any()) {
                _orchardServices.Notifier.Error(T("This step is unnecessary as some menus already exist."));
                return View();
            }

            // create a Main Menu
            var mainMenu = _menuService.Create("Main Menu");
            _orchardServices.Notifier.Information(T("Main menu created"));

            // assign the Main Menu to all current menu items
            foreach (var menuItem in _menuService.Get()) {
                // if they don't have a position or a text, then they are not displayed
                if (string.IsNullOrWhiteSpace(menuItem.MenuPosition) || string.IsNullOrEmpty(menuItem.MenuText)) {
                    continue;
                }
                menuItem.Menu = mainMenu.ContentItem;
            }
            _orchardServices.Notifier.Information(T("Menu items moved to Main menu"));

            // a widget should is created to display the navigation
            var layer = _widgetsService.GetLayers().FirstOrDefault(x => x.Name == "Default");
            if(layer == null) {
                _orchardServices.Notifier.Warning(T("Widget could not be created. Please create it manually."));
            }

            var widget = _widgetsService.CreateWidget(layer.Id, "MenuWidget", "Main Menu", "1.0", "Navigation");
            widget.RenderTitle = false;

            var menuWidget = widget.As<MenuWidgetPart>();

            menuWidget.Menu = mainMenu.ContentItem.Record;

            menuWidget.StartLevel = 1;
            menuWidget.Levels = 1;
            menuWidget.Breadcrumb = false;
            menuWidget.AddHomePage = false;
            menuWidget.AddCurrentPage = false;
            
            _orchardServices.ContentManager.Publish(menuWidget.ContentItem);
            
            return View("Index");
        }
    }
}
