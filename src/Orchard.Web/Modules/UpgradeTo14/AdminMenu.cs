using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace UpgradeTo14 {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Migrate Routes"), "0", item => item.Action("Index", "Admin", new { area = "UpgradeTo14" }).Permission(StandardPermissions.SiteOwner));
        }
    }
}
