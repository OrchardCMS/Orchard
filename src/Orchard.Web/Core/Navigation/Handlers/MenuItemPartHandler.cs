using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Navigation.Handlers {
    [UsedImplicitly]
    public class MenuItemPartHandler : ContentHandler {
        public MenuItemPartHandler(IRepository<MenuItemPartRecord> repository) {
            Filters.Add(new ActivatingFilter<MenuItemPart>("MenuItem"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}