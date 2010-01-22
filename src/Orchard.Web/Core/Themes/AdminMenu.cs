using Orchard.UI.Navigation;

namespace Orchard.Core.Themes {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Themes", "11",
                        menu => menu
                                    .Add("Manage Themes", "2.0", item => item.Action("Index", "Admin", new { area = "Themes" })
                                        .Permission(Permissions.ManageThemes).Permission(Permissions.ApplyTheme)));
        }
    }
}
