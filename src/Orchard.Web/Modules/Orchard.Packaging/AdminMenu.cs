using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;
using Orchard.Security;

namespace Orchard.Packaging {
    [OrchardFeature("Gallery")]
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Themes"), "25", menu => menu
                .Add(T("Available"), "1", item => item.Action("Themes", "Gallery", new { area = "Orchard.Packaging" })
                    .Permission(StandardPermissions.SiteOwner).LocalNav()));

            builder.Add(T("Modules"), "20", menu => menu
                .Add(T("Available"), "2", item => item.Action("Modules", "Gallery", new { area = "Orchard.Packaging" })
                    .Permission(StandardPermissions.SiteOwner).LocalNav()));

            builder.Add(T("Configuration"), "50", menu => menu
                .Add(T("Feeds"), "25", item => item.Action("Sources", "Gallery", new { area = "Orchard.Packaging" })
                    .Permission(StandardPermissions.SiteOwner)));
        }
    }
}