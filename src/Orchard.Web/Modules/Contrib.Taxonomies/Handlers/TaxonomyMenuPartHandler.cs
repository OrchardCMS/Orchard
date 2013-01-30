using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Contrib.Taxonomies.Models;

namespace Contrib.Taxonomies.Handlers {
    public class TaxonomyMenuPartHandler : ContentHandler {
        public TaxonomyMenuPartHandler(IRepository<TaxonomyMenuPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}