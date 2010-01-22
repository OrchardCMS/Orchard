using Orchard.UI.Navigation;

namespace Orchard.Core.Settings {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Settings", "11",
                        menu => menu
                                    .Add("Manage Settings", "2.0", item => item.Action("Index", "Admin", new { area = "Settings" }).Permission(Permissions.ManageSettings)));
        }
    }
}
