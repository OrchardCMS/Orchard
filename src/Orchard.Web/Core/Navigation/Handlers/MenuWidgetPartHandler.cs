using Orchard.ContentManagement.Handlers;
using Orchard.Core.Navigation.Models;

namespace Orchard.Core.Navigation.Handlers {
    public class MenuWidgetPartHandler : ContentHandler {
        public MenuWidgetPartHandler() {
            OnInitializing<MenuWidgetPart>((context, part) => { part.StartLevel = 1; });
        }
    }
}