using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Widgets {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("widgets")
                .Add(T("Widgets"), "5",
                    menu => menu.Add(T("Configure"), "0", item => item.Action("Index", "Admin", new { area = "Orchard.Widgets" })
                        .Permission(Permissions.ManageWidgets)));
        }
    }
}
