using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Packaging.Helpers;
using Orchard.UI.Navigation;

namespace Orchard.Packaging.Navigation {
    [OrchardFeature("Gallery")]
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .Add(T("Modules"),
                    menu => menu
                        .Add(T("Gallery"), "3", item => NavigationHelpers.Describe(item, "Modules", "Gallery", true)),
                    new[] {"plugin"})
                .Add(T("Themes"),
                    menu => menu
                        .Add(T("Gallery"), "3", item => NavigationHelpers.Describe(item, "Themes", "Gallery", true)),
                    new[] {"paint"});
        }
    }
}