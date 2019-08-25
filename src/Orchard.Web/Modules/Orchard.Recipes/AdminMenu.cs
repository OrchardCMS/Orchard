using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.Recipes {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName {
            get { return "admin"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("modules").Add(T("Modules"), "9", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu) {
            menu.Add(T("Recipes"), "2", item => item
                .Action("Index", "Admin", new { area = "Orchard.Recipes" })
                .Permission(StandardPermissions.SiteOwner).LocalNav());
        }
    }
}
