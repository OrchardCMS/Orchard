using Orchard.UI.Navigation;

namespace Orchard.Comments {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Comments", "3",
                        menu => menu
                                    .Add("Manage Comments", "1.0", item => item.Action("Index", "Admin", new { area = "Orchard.Comments" }).Permission(Permissions.ManageComments))
                                    );
        }
    }
}
