using Orchard.UI.Navigation;

namespace Orchard.Sandbox {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Sandbox", "9",
                        menu => menu
                                    .Add("List Sandbox Pages", "1.0", item => item.Action("Index", "Page", new { area = "Orchard.Sandbox" }))
                                    .Add("Create Sandbox Pages", "1.1", item => item.Action("Create", "Page", new { area = "Orchard.Sandbox" }))
                                    );
        }
    }
}
