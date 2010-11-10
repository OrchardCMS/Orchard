using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Packaging {
    [OrchardFeature("Gallery")]
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Gallery"), "30", menu => menu
                        .Add(T("Modules"), "1.0", item => item
                             .Action("ModulesIndex", "Gallery", new { area = "Orchard.Packaging" }))
                        .Add(T("Themes"), "2.0", item => item
                             .Action("ThemesIndex", "Gallery", new { area = "Orchard.Packaging" }))
                        .Add(T("Feeds"), "3.0", item => item
                             .Action("Sources", "Gallery", new { area = "Orchard.Packaging" })));
        }
    }
}