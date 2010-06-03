using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Users {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Users"), "5",
                        menu => menu
                                    .Add(T("Manage Users"), "1.0", item => item.Action("Index", "Admin", new { area = "Orchard.Users" }).Permission(Permissions.ManageUsers))
                                    .Add(T("Add New User"), "1.1", item => item.Action("Create", "Admin", new { area = "Orchard.Users" }).Permission(Permissions.ManageUsers)));
        }
    }
}
