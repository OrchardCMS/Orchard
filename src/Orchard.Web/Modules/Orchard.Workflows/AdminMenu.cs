using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.Workflows {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .AddImageSet("workflows")
                .Add(T("Workflows"), "10",
                menu => menu
                    .Add(T("Workflows"), "1.0",
                        qi => qi.Action("Index", "Admin", new { area = "Orchard.Workflows" }).Permission(StandardPermissions.SiteOwner).LocalNav())
            );
        }
    }
}
