using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Lists {
    public class AdminMenu : INavigationProvider {
        private const string ListContentTypeName = "List";
        private readonly IContentManager _contentManager;

        public AdminMenu(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("list")
                .Add(T("Lists"), "3", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu) {
            menu.Add(T("Manage Lists"), "1.0",
                item => item.Action("List", "Admin", new { area = "Contents", id = ListContentTypeName }));

            var ci = _contentManager.New(ListContentTypeName);
            var metadata = _contentManager.GetItemMetadata(ci);
            menu.Add(T("Create New List"), "1.5",
                item => item.Action(metadata.CreateRouteValues["Action"] as string, metadata.CreateRouteValues["Controller"] as string, metadata.CreateRouteValues));
        }
    }
}