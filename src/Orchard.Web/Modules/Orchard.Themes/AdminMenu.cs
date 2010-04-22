using Orchard.UI.Navigation;

namespace Orchard.Themes {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Site", "11",
                        menu => menu
                                    .Add("Manage Themes", "4.0", item => item.Action("Index", "Admin", new { area = "Orchard.Themes" })
                                                                             .Permission(Permissions.ManageThemes).Permission(Permissions.ApplyTheme)));
        }
    }
}