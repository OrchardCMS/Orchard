using Orchard.UI.Navigation;

namespace Orchard.Core.Navigation {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Navigation", "12",
                        menu => menu
                                    .Add("Manage Main Menu", "2.0", item => item.Action("Index", "Admin", new { area = "Navigation" }).Permission(Permissions.ManageMainMenu)));
        }
    }
}
