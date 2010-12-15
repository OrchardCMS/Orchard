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
            builder.Add(T("Gallery"), "30", menu => menu
                        .Add(T("Modules"), "1.0", item => item
                             .Action("Modules", "Gallery", new { area = "Orchard.Packaging" })
                             .Permission(StandardPermissions.SiteOwner))
                        .Add(T("Themes"), "2.0", item => item
                             .Action("Themes", "Gallery", new { area = "Orchard.Packaging" })
                             .Permission(StandardPermissions.SiteOwner))
                        .Add(T("Feeds"), "3.0", item => item
                             .Action("Sources", "Gallery", new { area = "Orchard.Packaging" })
                             .Permission(StandardPermissions.SiteOwner)));
        }
    }
}