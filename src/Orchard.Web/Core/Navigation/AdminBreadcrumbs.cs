using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Navigation;
using Orchard.Core.Navigation.Services;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Core.Navigation {
    public class AdminBreadcrumbs : AdminBreadcrumbsProvider {
        public const string Name = "Orchard.Core.Navigation.AdminBreadcrumbs";
        private readonly IOrchardServices _orchardServices;
        private readonly IMenuManager _menuManager;
        private UrlHelper _urlHelper;

        public AdminBreadcrumbs(IOrchardServices orchardServices, IMenuManager menuManager, UrlHelper urlHelper) {
            _orchardServices = orchardServices;
            _menuManager = menuManager;
            _urlHelper = urlHelper;
        }

        public override string MenuName {
            get { return Name; }
        }

        protected override void AddItems(NavigationItemBuilder root) {
            root.Add(T("Navigation"), navigation => {
                navigation.Action("Index", "Admin", new { area = "Navigation", menuId = default(int?) });

                var menus = _orchardServices.ContentManager.Query("Menu").List().ToList();
                var allowedMenus = menus.Where(menu => _orchardServices.Authorizer.Authorize(Permissions.ManageMenus, menu)).ToList();
                var menuItemTypes = _menuManager.GetMenuItemTypes();

                foreach (var menu in allowedMenus) {
                    var menuMetadata = _orchardServices.ContentManager.GetItemMetadata(menu);
                    var returnUrl = _urlHelper.RouteUrl(menuMetadata.AdminRouteValues);

                    navigation.Add(new LocalizedString(menuMetadata.DisplayText), m => {
                        m.Action(menuMetadata.AdminRouteValues);

                        foreach(var menuItemType in menuItemTypes) {
                            m.Add(new LocalizedString(menuItemType.DisplayName), itemType => itemType.Action("CreateMenuItem", "Admin", new { area = "Navigation", id = menuItemType.Type, menuId = menu.Id, returnUrl = returnUrl }));
                        }
                    });
                }
            });
        }
    }
}