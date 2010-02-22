using Orchard.UI.Navigation;

namespace Orchard.Media {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Media", "4",
                        menu => menu
                                    .Add("Manage Media", "1.0", item => item.Action("Index", "Admin", new { area = "Orchard.Media" }).Permission(Permissions.ManageMediaFiles))
                                    );
        }
    }
}
