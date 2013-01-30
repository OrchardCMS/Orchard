using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Contrib.Taxonomies.Models;
using Orchard.Environment.Extensions;

namespace Contrib.Taxonomies.Handlers {
    [OrchardFeature("TaxonomyMenuItem")]
    public class TaxonomyMenuItemPartHandler : ContentHandler {
        public TaxonomyMenuItemPartHandler(IRepository<TaxonomyMenuItemPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<TaxonomyMenuItemPart>("Taxonomy"));
        }
    }
}