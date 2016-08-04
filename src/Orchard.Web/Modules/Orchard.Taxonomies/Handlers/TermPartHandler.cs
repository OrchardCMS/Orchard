using Orchard.Taxonomies.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.Taxonomies.Handlers {
    public class TermPartHandler : ContentHandler {
        public TermPartHandler(IRepository<TermPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            OnInitializing<TermPart>((context, part) => part.Selectable = true);
        }
    }
}