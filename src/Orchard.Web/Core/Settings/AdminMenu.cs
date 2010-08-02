using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Core.Settings {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Site Configuration"), "11",
                        menu => menu
                                    .Add(T("Settings"), "10", item => item.Action("Index", "Admin", new { area = "Settings" }).Permission(Permissions.ManageSettings)));
        }
    }
}
