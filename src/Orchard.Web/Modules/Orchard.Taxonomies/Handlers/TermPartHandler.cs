using Orchard.Taxonomies.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System.Web.Routing;
using Orchard.ContentManagement;

namespace Orchard.Taxonomies.Handlers {
    public class TermPartHandler : ContentHandler {
        public TermPartHandler(IRepository<TermPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            OnInitializing<TermPart>((context, part) => part.Selectable = true);
        }
        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var term = context.ContentItem.As<TermPart>();

            if (term == null)
                return;

            context.Metadata.EditorRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Taxonomies"},
                {"Controller", "TermAdmin"},
                {"Action", "Edit"},
                {"Id", term.Id}
            };

        }
    }
}