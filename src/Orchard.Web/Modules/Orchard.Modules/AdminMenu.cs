using Orchard.UI.Navigation;

namespace Orchard.Modules {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Features", "10",
                        menu => menu
                                    .Add("Manage Features", "1.0", item => item.Action("Features", "Admin", new { area = "Orchard.Modules" })
                                        .Permission(Permissions.ManageFeatures))
                                    .Add("Manage Modules", "2.0", item => item.Action("Index", "Admin", new { area = "Orchard.Modules" })
                                        .Permission(Permissions.ManageModules)));
        }
    }
}