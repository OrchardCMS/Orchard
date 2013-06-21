using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Upgrade {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .Add(T("Upgrade to 1.7"), "0", menu => menu.Action("Index", "Route", new { area = "Upgrade" })
                    .Add(T("Migrate Media"), "1", item => item.Action("Index", "Media", new { area = "Upgrade" }).LocalNav().Permission(StandardPermissions.SiteOwner))
                    .Add(T("Migrate Taxonomies"), "2", item => item.Action("Index", "Taxonomy", new { area = "Upgrade" }).LocalNav().Permission(StandardPermissions.SiteOwner))
                    .Add(T("Migrate Routes"), "3", item => item.Action("Index", "Route", new { area = "Upgrade" }).LocalNav().Permission(StandardPermissions.SiteOwner))
                    .Add(T("Migrate Fields"), "4", item => item.Action("Index", "Field", new { area = "Upgrade" }).LocalNav().Permission(StandardPermissions.SiteOwner))
                    .Add(T("Migrate Menu"), "5", item => item.Action("Index", "Menu", new { area = "Upgrade" }).LocalNav().Permission(StandardPermissions.SiteOwner))
                );
        }
    }
}
