using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.Core.Dashboard {
    // We don't want to display the Dashboard in the "admin" menu, but we need to register this menu item nonetheless
    // as it needs to be the root of the menu structure.
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("dashboard")
                .Add(T("Dashboard"), "-5",
                    item => item
                        .Action("Index", "Admin", new { area = "Dashboard" })
                        .Permission(StandardPermissions.AccessAdminPanel), new[] { "hidden" });
        }
    }
}