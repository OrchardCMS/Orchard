using Orchard.Core.Navigation.Models;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Navigation.Handlers {
    public class MenuItemPartHandler : ContentHandler {
        public MenuItemPartHandler() {
            Filters.Add(new ActivatingFilter<MenuItemPart>("MenuItem"));
        }
    }
}