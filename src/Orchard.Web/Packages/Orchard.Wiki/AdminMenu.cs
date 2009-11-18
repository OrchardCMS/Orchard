using Orchard.UI.Navigation;

namespace Orchard.Wikis {
    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Wiki", "9",
                        menu => menu
                                    .Add("Wiki Pages", "1.0", item => item.Action("Index", "Admin", new { area = "Orchard.Wiki" }))
                                    );
        }
    }
}
