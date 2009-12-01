using Orchard.UI.Navigation;

namespace Orchard.CmsPages {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Pages", "1",
                        menu => menu
                                    .Add("Manage Pages", "1.0", item => item.Action("Index", "Admin", new { area = "Orchard.CmsPages" }))
                                    .Add("Add a Page", "1.1", item => item.Action("Create", "Admin", new { area = "Orchard.CmsPages" }).Permission(Permissions.CreatePages))
                                    );
        }
    }
}
