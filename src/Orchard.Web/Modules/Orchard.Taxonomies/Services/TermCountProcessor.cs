using Orchard.ContentManagement;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Services {
    public class TermCountProcessor : ITermCountProcessor {
        private readonly ITaxonomyService _taxonomyService;

        public TermCountProcessor(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
        }
        
        public void Process(params int[] termPartRecordIds) {

            foreach (var id in termPartRecordIds) {
                var termPart = _taxonomyService.GetTerm(id);
                while (termPart != null) {
                    termPart.Count = (int)_taxonomyService.GetContentItemsCount(termPart);

                    // compute count for the hierarchy too
                    if (termPart.Container != null) {
                        var parentTerm = termPart.Container.As<TermPart>();
                        termPart = parentTerm;
                    }
                }
            }
        }
    }
}
