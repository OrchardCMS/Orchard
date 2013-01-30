using Contrib.Taxonomies.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Contrib.Taxonomies.Handlers {
    public class TermWidgetPartHandler : ContentHandler {
        public TermWidgetPartHandler(IRepository<TermWidgetPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}