using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Navigation;
using System.Linq;

namespace Orchard.Core.Navigation {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public IOrchardServices Services { get; set; }

        public AdminMenu(IOrchardServices services) {
            Services = services;
        }

        public void GetNavigation(NavigationBuilder builder) {
            var menus = Services.ContentManager.Query("Menu").List();
            var allowedMenus = menus.Where(menu => Services.Authorizer.Authorize(Permissions.ManageMenus, menu)).ToList();

            // If the current user cannot manage menus, check if they can manage at least one.
            if (!Services.Authorizer.Authorize(Permissions.ManageMenus)) {
                if (!allowedMenus.Any())
                    return;
            }

            builder
                .AddImageSet("navigation")
                .Add(T("Navigation"), "7", navigation => {
                    navigation.Action("Index", "Admin", new { area = "Navigation" });

                    if (allowedMenus.Any()) {
                        navigation.Add(T("All Menus"), allMenus => allMenus.Action("Index", "Admin", new { area = "Navigation" }));

                        foreach (var menu in allowedMenus) {
                            var menuMetadata = Services.ContentManager.GetItemMetadata(menu);
                            navigation.Add(new LocalizedString(menuMetadata.DisplayText), x => x.Action(menuMetadata.AdminRouteValues));
                        }
                    }
                });
        }
    }
}
