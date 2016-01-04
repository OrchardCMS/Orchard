using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Packaging.Helpers;
using Orchard.UI.Navigation;

namespace Orchard.Packaging.Navigation {
    [OrchardFeature("Gallery")]
    public class TopAdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName {
            get { return "top_admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .Add(T("Settings"), menu => menu
                    .Add(T("Gallery"), "1", item => NavigationHelpers.Describe(item, "Sources", "Gallery", false)));
        }
    }
}