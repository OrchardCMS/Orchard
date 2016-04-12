using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Navigation.Models;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Navigation.Handlers {
    public class MenuItemPartHandler : ContentHandler {
        public MenuItemPartHandler() {
            Filters.Add(new ActivatingFilter<MenuItemPart>("MenuItem"));
            
            OnUpdated<MenuItemPart>(AttachMenu);
        }

        private void AttachMenu(UpdateContentContext context, MenuItemPart part) {
            // Check if the menu item is localizable.
            var localizableAspect = part.As<ILocalizableAspect>();

            if (localizableAspect == null)
                return;

            // Check if this is the master content item.
            if (localizableAspect.MasterContentItem == part)
                return;

            // This is a localized version of the menu item, so ensure it is bound to the same Menu.
            var menuPart = part.As<MenuPart>();
            menuPart.Menu = localizableAspect.MasterContentItem.As<MenuPart>().Menu;
        }
    }
}