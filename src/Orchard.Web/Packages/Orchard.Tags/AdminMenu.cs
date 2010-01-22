using Orchard.UI.Navigation;

namespace Orchard.Tags {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Tags", "3",
                        menu => menu
                                    .Add("Manage Tags", "1.0", item => item.Action("Index", "Admin", new { area = "Orchard.Tags" }).Permission(Permissions.ManageTags))
                                    );
        }
    }
}
