using Orchard.Pages.Services;
using Orchard.UI.Navigation;

namespace Orchard.Pages {
    public class AdminMenu : INavigationProvider {
        private readonly IPageService _pageService;

        public AdminMenu(IPageService pageService) {
            _pageService = pageService;
        }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add("Pages", "1", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu) {
            if (_pageService.GetCount() > 0)
                menu.Add("Manage Pages", "1.0",
                         item =>
                         item.Action("List", "Admin", new {area = "Orchard.Pages"}).Permission(Permissions.MetaListPages));

            menu.Add("Add New Page", "1.1",
                     item =>
                     item.Action("Create", "Admin", new {area = "Orchard.Pages"}).Permission(Permissions.EditPages));
        }
    }
}
