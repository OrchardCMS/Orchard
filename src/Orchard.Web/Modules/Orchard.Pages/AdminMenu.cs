using Orchard.Localization;
using Orchard.Pages.Services;
using Orchard.UI.Navigation;

namespace Orchard.Pages {
    public class AdminMenu : INavigationProvider {
        private readonly IPageService _pageService;

        public AdminMenu(IPageService pageService) {
            _pageService = pageService;
        }

        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Pages"), "1", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu) {
            if (_pageService.GetCount() > 0)
                menu.Add(T("Manage Pages"), "1.0",
                         item =>
                         item.Action("List", "Admin", new {area = "Orchard.Pages"}).Permission(Permissions.MetaListPages));

            menu.Add(T("Create New Page"), "1.1",
                     item =>
                     item.Action("Create", "Admin", new {area = "Orchard.Pages"}).Permission(Permissions.EditPages));
        }
    }
}
