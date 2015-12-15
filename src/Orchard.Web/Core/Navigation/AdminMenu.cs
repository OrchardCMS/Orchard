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
            var user = Services.WorkContext.CurrentUser;

            // if the current user cannot manage menus, check if they can manage at least one
            if (!Services.Authorizer.Authorize(Permissions.ManageMenus)) { 
                var menus = Services.ContentManager.Query("Menu").List();

                if (!menus.Any(x => Services.Authorizer.Authorize(Permissions.ManageMenus, x))) {
                    return;
                }
            }

            builder.AddImageSet("navigation")
                .Add(T("Navigation"), "7",
                    menu => menu
                        .Add(T("Main Menu"), "0", item => item.Action("Index", "Admin", new { area = "Navigation" })
                        ));
        }
    }
}
