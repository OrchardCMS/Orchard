using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Packaging {
    [OrchardFeature("Gallery")]
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Site Configuration"), "11", menu => menu
                        .Add(T("Gallery"), "5.0", item => item
                             .Action("Index", "Packaging", new { area = "Orchard.Packaging" })));
        }
    }
}