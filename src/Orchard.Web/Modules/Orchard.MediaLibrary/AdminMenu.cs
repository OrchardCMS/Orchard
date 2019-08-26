using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.MediaLibrary {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public AdminMenu() {
            T = NullLocalizer.Instance;
        }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("media-library")
                .Add(T("Media"), "6",
                    menu => menu.Add(T("Media"), "0", item => item.Action("Index", "Admin", new { area = "Orchard.MediaLibrary" })
                        .Permission(Permissions.ManageOwnMedia)
                        .Permission(Permissions.SelectMediaContent)));
        }
    }
}