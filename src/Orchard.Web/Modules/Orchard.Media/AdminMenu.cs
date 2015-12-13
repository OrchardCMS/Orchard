using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Media {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public AdminMenu() {
            T = NullLocalizer.Instance;
        }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("media")
                .Add(T("Media"), "6",
                    menu => menu.Add(T("Media Storage"), "1", item => item.Action("Index", "Admin", new { area = "Orchard.Media" })
                        .Permission(Permissions.ManageMedia)));
        }
    }
}