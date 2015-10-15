using Orchard.UI.Navigation;

namespace Orchard.Templates {
    public class AdminMenu : Component, INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .AddImageSet("templates")
                .Add(T("Templates"), "5.0", item => item.Action("List", "Admin", new { area = "Orchard.Templates", id = "" }).Permission(Permissions.ManageTemplates));
        }
    }
}