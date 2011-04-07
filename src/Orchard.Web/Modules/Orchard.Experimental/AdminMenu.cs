using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Experimental {

    public class AdminMenu : INavigationProvider {
        public string MenuName { get { return "admin"; } }
        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("experimental")
                .Add(T("Experimental"), "60",
                    menu => menu.Add(T("Debug"), "0", item => item.Action("Index", "Home", new { area = "Orchard.Experimental" })));
        }
    }
}