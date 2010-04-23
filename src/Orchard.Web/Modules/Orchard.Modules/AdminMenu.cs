using Orchard.UI.Navigation;

namespace Orchard.Modules {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Modules", "10",
                        menu => menu
                                    .Add("Manage Modules", "1.0", item => item.Action("Index", "Admin", new { area = "Orchard.Modules" })
                                                                             .Permission(Permissions.ManageModules)));
        }
    }
}