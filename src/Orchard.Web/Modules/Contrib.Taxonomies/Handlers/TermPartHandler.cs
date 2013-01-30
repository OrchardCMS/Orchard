using Contrib.Taxonomies.Routing;
using Contrib.Taxonomies.Services;
using Contrib.Taxonomies.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Contrib.Taxonomies.Handlers {
    public class TermPartHandler : ContentHandler {
        public TermPartHandler(
            IRepository<TermPartRecord> repository, 
            ITaxonomyService taxonomyService,
            ITermPathConstraint termPathConstraint ) {
            Filters.Add(StorageFilter.For(repository));

            OnRemoved<IContent>(
                (context, tags) =>
                    taxonomyService.DeleteAssociatedTerms(context.ContentItem)
                );

            OnInitializing<TermPart>(
                (context, part) => 
                    part.Selectable = true
                );

            OnPublished<TermPart>(
                (context, part) => {
                    termPathConstraint.AddPath(part.Slug);
                    foreach (var child in taxonomyService.GetChildren(part)) {
                        termPathConstraint.AddPath(child.Slug);
                    }
                });

            OnUnpublishing<TermPart>(
                (context, part) =>
                    termPathConstraint.RemovePath(part.Slug)
                );
        }
    }
}