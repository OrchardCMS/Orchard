using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace UpgradeTo16 {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .Add(T("Upgrade to 1.6"), "0", menu => menu.Action("Index", "Route", new { area = "UpgradeTo16" })
                    .Add(T("Migrate Routes"), "0", item => item.Action("Index", "Route", new { area = "UpgradeTo16" }).LocalNav().Permission(StandardPermissions.SiteOwner))
                    .Add(T("Migrate Fields"), "0", item => item.Action("Index", "Field", new { area = "UpgradeTo16" }).LocalNav().Permission(StandardPermissions.SiteOwner))
                    .Add(T("Migrate Menu"), "0", item => item.Action("Index", "Menu", new { area = "UpgradeTo16" }).LocalNav().Permission(StandardPermissions.SiteOwner))
                );
        }
    }
}
