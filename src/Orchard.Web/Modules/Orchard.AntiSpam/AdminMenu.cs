using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.AntiSpam {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Spam"), "11",
                        menu => menu
                                    .Add(T("Manage Spam"), "1.0", item => item.Action("Index", "Admin", new { area = "Orchard.AntiSpam" }).Permission(Permissions.ManageAntiSpam))
                                    );
        }
    }
}
