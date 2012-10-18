using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Navigation.Models;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Core.Navigation.Services {
    public class DefaultMenuProvider : IMenuProvider {
        private readonly IContentManager _contentManager;

        public DefaultMenuProvider(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void GetMenu(IContent menu, NavigationBuilder builder) {
            var menuParts = _contentManager
                .Query<MenuPart, MenuPartRecord>()
                .Where(x => x.MenuId == menu.Id)
                .WithQueryHints(new QueryHints().ExpandRecords<MenuItemPartRecord>())
                .List();

            foreach (var menuPart in menuParts) {
                if (menuPart != null) {
                    var part = menuPart;

                    string culture = null;
                    // fetch the culture of the content menu item, if any
                    var localized = part.As<ILocalizableAspect>();
                    if (localized != null) {
                        culture = localized.Culture;
                    }
                    else {
                        // fetch the culture of the content menu item, if any
                        var contentMenuItemPart = part.As<ContentMenuItemPart>();
                        if (contentMenuItemPart != null) {
                            localized = contentMenuItemPart.Content.As<ILocalizableAspect>();
                            if (localized != null) {
                                culture = localized.Culture;
                            }
                        }
                    }

                    if (part.Is<MenuItemPart>())
                        builder.Add(new LocalizedString(HttpUtility.HtmlEncode(part.MenuText)), part.MenuPosition, item => item.Url(part.As<MenuItemPart>().Url).Content(part).Culture(culture));
                    else
                        builder.Add(new LocalizedString(HttpUtility.HtmlEncode(part.MenuText)), part.MenuPosition, item => item.Action(_contentManager.GetItemMetadata(part.ContentItem).DisplayRouteValues).Content(part).Culture(culture));
                }
            }
        }
    }
}