using Orchard.UI.Navigation;

namespace Orchard.Core.Navigation {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Site", "12",
                        menu => menu
                                    .Add("Manage Menu", "6.0", item => item.Action("Index", "Admin", new { area = "Navigation" }).Permission(Permissions.ManageMainMenu)));
        }
    }
}
