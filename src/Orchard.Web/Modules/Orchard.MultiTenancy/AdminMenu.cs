using Orchard.UI.Navigation;

namespace Orchard.MultiTenancy {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Tenants", "22",
                        menu => menu
                                    .Add("Manage Tenants", "1.0", item => item.Action("Index", "Admin", new { area = "Orchard.MultiTenancy" }).Permission(Permissions.ManageTenants))
                                    .Add("Add New Tenant", "1.1", item => item.Action("Add", "Admin", new { area = "Orchard.MultiTenancy" }).Permission(Permissions.ManageTenants)));
        }
    }
}
