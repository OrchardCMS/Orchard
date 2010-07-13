using Orchard.ContentManagement.Handlers;
using Orchard.Core.Routable.Models;
using Orchard.Data;

namespace Orchard.Core.Routable.Handlers {
    public class RoutableHandler : ContentHandler {
        public RoutableHandler(IRepository<RoutableRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
