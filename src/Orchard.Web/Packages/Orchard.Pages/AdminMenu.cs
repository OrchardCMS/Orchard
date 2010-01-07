using Orchard.UI.Navigation;

namespace Orchard.Pages {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Pages", "1",
                        menu => menu
                                    .Add("Manage Pages", "1.0", item => item.Action("List", "Admin", new { area = "Orchard.Pages" }))
                                    .Add("Add New Page", "1.1", item => item.Action("Create", "Admin", new { area = "Orchard.Pages" })));
        }
    }
}
