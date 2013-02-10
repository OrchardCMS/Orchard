using Orchard.Core.Navigation.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Navigation.Handlers {
    public class ShapeMenuItemPartHandler : ContentHandler {
        public ShapeMenuItemPartHandler(IRepository<ShapeMenuItemPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}