using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Packaging {
    [OrchardFeature("Gallery")]
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Gallery"), "5", menu => menu
                        .Add(T("Browse Gallery"), "1.0", item => item
                             .Action("Index", "Gallery", new { area = "Orchard.Packaging" }))
                        .Add(T("Gallery Feeds"), "2.0", item => item
                             .Action("Sources", "Gallery", new { area = "Orchard.Packaging" })));
        }
    }
}