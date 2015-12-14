using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Search {
    [OrchardFeature("Orchard.Search.Content")]
    public class ContentAdminMenu : INavigationProvider {
        public ContentAdminMenu() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Content"),
                menu => menu
                    .Add(T("Search"), "1.5", item => item.Action("Index", "Admin", new {area = "Orchard.Search"}).LocalNav())
                );

        }
    }
}