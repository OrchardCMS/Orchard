using Orchard.UI.Navigation;

namespace Orchard.Messaging {
    public class AdminMenu : Component, INavigationProvider {

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .AddImageSet("messaging")
                .Add(T("Message Queue"), "15.0", item => {
                    item.Action("List", "Admin", new { area = "Orchard.Messaging" });
                    item.LinkToFirstChild(false);
                });
        }
    }
}