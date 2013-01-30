using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Contrib.Taxonomies {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Taxonomies"), "4",
                        menu => menu
                                    .Add(T("Manage Taxonomies"), "1.0", item => item.Action("Index", "Admin", new { area = "Contrib.Taxonomies" }).Permission(Permissions.ManageTaxonomies))
                                    );
        }
    }
}
