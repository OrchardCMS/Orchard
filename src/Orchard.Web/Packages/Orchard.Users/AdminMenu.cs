using Orchard.UI.Navigation;

namespace Orchard.Users {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Users", "5",
                        menu => menu
                                    .Add("Manage Users", "1.0", item => item.Action("Index", "Admin", new { area = "Orchard.Users" }))
                                    .Add("Create New User", "1.1", item => item.Action("Create", "Admin", new { area = "Orchard.Users" })));
        }
    }
}
