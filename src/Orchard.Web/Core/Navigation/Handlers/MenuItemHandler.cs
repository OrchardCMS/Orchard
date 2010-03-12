using JetBrains.Annotations;
using Orchard.Core.Navigation.Drivers;
using Orchard.Core.Navigation.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Navigation.Handlers {
    [UsedImplicitly]
    public class MenuItemHandler : ContentHandler {
        public MenuItemHandler(IRepository<MenuItemRecord> repository) {
            Filters.Add(new ActivatingFilter<MenuItem>(MenuItemDriver.ContentType.Name));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}