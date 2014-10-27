using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Layouts {
    public class AdminMenu : INavigationProvider {

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .AddImageSet("layouts")
                .Add(T("Elements"), "8.5", menu => menu.Action("Index", "BlueprintAdmin", new { area = "Orchard.Layouts" }));
        }
    }
}