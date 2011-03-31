using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Core.Navigation {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            //todo: - add new menu? and list menus? ...and remove hard-coded menu name here
            builder.AddImageSet("navigation")
                .Add(T("Navigation"), "7",
                    menu => menu
                        .Add(T("Main Menu"), "0", item => item.Action("Index", "Admin", new { area = "Navigation" })
                        .Permission(Permissions.ManageMainMenu)));
        }
    }
}
