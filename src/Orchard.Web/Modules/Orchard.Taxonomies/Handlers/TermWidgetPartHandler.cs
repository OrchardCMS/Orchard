using Orchard.Taxonomies.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.Taxonomies.Handlers {
    public class TermWidgetPartHandler : ContentHandler {
        public TermWidgetPartHandler(IRepository<TermWidgetPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}