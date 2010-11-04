using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.Core.Reports {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Configuration"), "50",
                menu => menu.Add(T("Reports"), "20", item => item.Action("Index", "Admin", new { area = "Reports" })
                    .Permission(StandardPermissions.AccessAdminPanel)));
        }
    }
}