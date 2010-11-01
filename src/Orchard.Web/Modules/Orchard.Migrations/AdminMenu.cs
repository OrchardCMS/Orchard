using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Migrations {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Developer"), "1",
                menu => menu.Add(T("Migration"), "1.0", item => item.Action("Index", "DatabaseUpdate", new { area = "Orchard.Migrations" })));
        }
    }
}