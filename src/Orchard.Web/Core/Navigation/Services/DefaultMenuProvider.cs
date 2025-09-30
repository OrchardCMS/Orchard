using System.Collections.Generic;
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

            _menuPartsMemory = new Dictionary<int, IEnumerable<MenuPart>>();
        }

        // Prevent doing the same query for MenuParts more than once on a same request
        // in case we are building the same menu several times.
        private Dictionary<int, IEnumerable<MenuPart>> _menuPartsMemory;

        public void GetMenu(IContent menu, NavigationBuilder builder) {
            if (!_menuPartsMemory.ContainsKey(menu.Id)) {
                _menuPartsMemory[menu.Id] = _contentManager
                    .Query<MenuPart, MenuPartRecord>()
                    .Where(x => x.MenuId == menu.Id)
                    .List();
            }
            var menuParts = _menuPartsMemory[menu.Id];

            foreach (var menuPart in menuParts) {
                if (menuPart != null) {
                    var part = menuPart;

                    var showItem = true;
                    // If the menu item is a ContentMenuItemPart (from Orchard.ContentPicker), check the ContentItem is published.
                    // If there is no published version of the ContentItem, the item must not be added to NavigationBuilder.
                    var cmip = ((dynamic)part).ContentMenuItemPart;
                    if (cmip != null) {
                        showItem = cmip.Content != null;
                    }

                    if (showItem) {
                        string culture = null;
                        // fetch the culture of the content menu item, if any
                        var localized = part.As<ILocalizableAspect>();
                        if (localized != null) {
                            culture = localized.Culture;
                        }

                        if (part.Is<MenuItemPart>())
                            builder.Add(new LocalizedString(HttpUtility.HtmlEncode(part.MenuText)), part.MenuPosition, item => item.Url(part.As<MenuItemPart>().Url).Content(part).Culture(culture).Permission(Contents.Permissions.ViewContent));
                        else
                            builder.Add(new LocalizedString(HttpUtility.HtmlEncode(part.MenuText)), part.MenuPosition, item => item.Action(_contentManager.GetItemMetadata(part.ContentItem).DisplayRouteValues).Content(part).Culture(culture).Permission(Contents.Permissions.ViewContent));
                    }
                }
            }
        }
    }
}