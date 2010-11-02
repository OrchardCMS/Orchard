using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Indexing {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Configuration"), "50",
                menu => menu.Add(T("Search Index"), "15", item => item.Action("Index", "Admin", new {area = "Orchard.Indexing"})
                    .Permission(Permissions.ManageSearchIndex)));
        }
    }
}