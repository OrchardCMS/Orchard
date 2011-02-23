using Orchard.Localization;
using Orchard.UI.Navigation;
using Orchard.Security;

namespace Orchard.Roles {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Users"),
                menu => menu.Add(T("Roles"), "2.0", item => item.Action("Index", "Admin", new { area = "Orchard.Roles" })
                    .LocalNav().Permission(StandardPermissions.SiteOwner)));
        }
    }
}
