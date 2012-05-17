using Orchard.ContentManagement.Handlers;
using Orchard.Core.Navigation.Models;
using Orchard.Data;

namespace Orchard.Core.Navigation.Handlers {
    public class MenuWidgetPartHandler : ContentHandler {
        public MenuWidgetPartHandler(IRepository<MenuWidgetPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));

            OnInitializing<MenuWidgetPart>((context, part) => { part.StartLevel = 1; });
        }
    }
}