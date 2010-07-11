using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Futures.Modules.Packaging {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Site Configuration"), "11", menu => menu
                        .Add(T("Module Packaging"), "5.0", item => item
                             .Action("Index", "Packaging", new { area = "Futures.Modules.Packaging" })));
        }
    }
}