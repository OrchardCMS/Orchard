using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.Core.Dashboard {
    // We want to display the Dashboard menu item in the "top_admin" menu.
    public class TopAdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "top_admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("dashboard")
                .Add(T("Dashboard"), "-5",
                    menu => menu.Add(T("Orchard"), "-5",
                        item => item
                            .Action("Index", "Admin", new { area = "Dashboard" })
                            .Permission(StandardPermissions.AccessAdminPanel)));
        }
    }
}