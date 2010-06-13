using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Core.Contents {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Content"), "1",
                        menu => {
                            menu.Add(T("Create New Content"), "1.1", item => item.Action("Create", "Admin", new { area = "Contents" }));
                            menu.Add(T("Manage Content"), "1.2", item => item.Action("List", "Admin", new { area = "Contents" }));
                        });
            builder.Add(T("Site Configuration"), "11",
                        menu => menu.Add(T("Content Types"), "3", item => item.Action("Types", "Admin", new { area = "Contents" })));
        }
    }
}