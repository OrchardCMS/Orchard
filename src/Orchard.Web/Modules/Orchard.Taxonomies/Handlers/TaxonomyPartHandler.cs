using Orchard.Taxonomies.Services;
using JetBrains.Annotations;
using Orchard.Taxonomies.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.Taxonomies.Handlers {
    [UsedImplicitly]
    public class TaxonomyPartHandler : ContentHandler {
        public TaxonomyPartHandler(IRepository<TaxonomyPartRecord> repository, ITaxonomyService taxonomyService) {
            Filters.Add(StorageFilter.For(repository));
            OnPublished<TaxonomyPart>((context, part) => taxonomyService.CreateTermContentType(part));
            OnLoading<TaxonomyPart>( (context, part) => part.TermsField.Loader(x => taxonomyService.GetTerms(part.Id)));
        }
    }
}