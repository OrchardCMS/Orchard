using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.Rules {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Rules"), "4",
                menu => menu
                    .Add(T("Manage Rules"), "1.0",
                        item => item.Action("Index", "Admin", new { area = "Orchard.Rules" }).Permission(StandardPermissions.SiteOwner))
            );
        }
    }
}
