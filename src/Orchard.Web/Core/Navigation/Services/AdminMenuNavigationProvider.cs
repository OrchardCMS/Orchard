using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Navigation.Models;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Core.Navigation.Services {
    public class AdminMenuNavigationProvider : INavigationProvider {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public AdminMenuNavigationProvider(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager) {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            var menuParts = _contentManager.Query<AdminMenuPart, AdminMenuPartRecord>().Where(x => x.OnAdminMenu).List();
            foreach (var menuPart in menuParts) {
                if (menuPart != null) {
                    var part = menuPart;

                    builder.Add(new LocalizedString(part.AdminMenuText),
                                part.AdminMenuPosition,
                                item => item.Action(_contentManager.GetItemMetadata(part.ContentItem).AdminRouteValues));
                    // todo: somehow determine if they will ultimately have rights to the destination and hide if not. possibly would need to add a Permission to metadata.
                    // todo: give an iconset somehow (e.g. based on convention, module/content/<content-type>.adminmenu.png).
                }
            }
        }
    }
}