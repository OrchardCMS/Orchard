using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Media {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Media"), "4",
                        menu => menu
                                    .Add(T("Manage Media"), "1.0", item => item.Action("Index", "Admin", new { area = "Orchard.Media" }).Permission(Permissions.ManageMediaFiles))
                                    );
        }
    }
}
