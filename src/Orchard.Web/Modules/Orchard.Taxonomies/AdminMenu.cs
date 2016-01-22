using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Taxonomies {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .AddImageSet("taxonomies")
                .Add(T("Taxonomies"), "4", menu => menu
                .Add(T("Manage Taxonomies"), "1.0", item => item.Action("Index", "Admin", new { area = "Orchard.Taxonomies" }).Permission(Permissions.ManageTaxonomies))
            );
        }
    }
}
