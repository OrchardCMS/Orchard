using System.Web;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Core.Navigation.Services {
    [UsedImplicitly]
    public class MainMenuNavigationProvider : INavigationProvider {
        private readonly IContentManager _contentManager;

        public MainMenuNavigationProvider(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public string MenuName { get { return "main"; } }

        public void GetNavigation(NavigationBuilder builder) {
            var menuParts = _contentManager.Query<MenuPart, MenuPartRecord>().Where(x => x.OnMainMenu).WithQueryHints(new QueryHints().ExpandRecords<MenuItemPartRecord>()).List();
            foreach (var menuPart in menuParts) {
                if (menuPart != null) {
                    var part = menuPart;

                    if (part.Is<MenuItemPart>())
                        builder.Add(new LocalizedString(HttpUtility.HtmlEncode(part.MenuText)), part.MenuPosition, item => item.Url(part.As<MenuItemPart>().Url));
                    else
                        builder.Add(new LocalizedString(HttpUtility.HtmlEncode(part.MenuText)), part.MenuPosition, item => item.Action(_contentManager.GetItemMetadata(part.ContentItem).DisplayRouteValues));
                }
            }
        }
    }
}