using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Roles {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Users"), "5",
                        menu => menu
                                    .Add(T("Manage Roles"), "2.0", item => item.Action("Index", "Admin", new { area = "Orchard.Roles" }).Permission(Permissions.ManageRoles))
                                    .Add(T("Add New Role"), "2.1", item => item.Action("Create", "Admin", new { area = "Orchard.Roles" }).Permission(Permissions.ManageRoles)));
        }
    }
}
