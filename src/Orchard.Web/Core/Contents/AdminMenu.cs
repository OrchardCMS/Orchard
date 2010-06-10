using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.Core.Contents {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Content"), "1",
                        menu => {
                            menu.Add(T("Create"), "1.1", item => item.Action("Create", "Admin", new { area = "Contents" }));
                            menu.Add(T("List"), "1.2", item => item.Action("List", "Admin", new { area = "Contents" }));
                            menu.Add(T("Types"), "1.3", item => item.Action("Types", "Admin", new { area = "Contents" }));
                        });
        }
    }
}