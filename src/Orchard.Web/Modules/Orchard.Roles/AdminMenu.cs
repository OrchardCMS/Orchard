using Orchard.UI.Navigation;

namespace Orchard.Roles {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Users", "5",
                        menu => menu
                                    .Add("Manage Roles", "2.0", item => item.Action("Index", "Admin", new { area = "Orchard.Roles" }).Permission(Permissions.ManageRoles))
                                    .Add("Add New Role", "2.1", item => item.Action("Create", "Admin", new { area = "Orchard.Roles" }).Permission(Permissions.ManageRoles)));
        }
    }
}
