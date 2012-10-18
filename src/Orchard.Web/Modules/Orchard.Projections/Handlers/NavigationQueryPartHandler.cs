using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Projections.Models;

namespace Orchard.Projections.Handlers {
    public class NavigationQueryPartHandler : ContentHandler {
        public NavigationQueryPartHandler(IRepository<NavigationQueryPartRecord> navigationQueryRepository) {
            Filters.Add(StorageFilter.For(navigationQueryRepository));
        }
    }
}