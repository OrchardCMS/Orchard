using Orchard.ContentManagement;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Services {
    public class TaxonomySource : ITaxonomySource {
        private readonly ITaxonomyService _taxonomyService;
        public TaxonomySource(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
        }

        public TaxonomyPart GetTaxonomy(string name, ContentItem item) {
            return _taxonomyService.GetTaxonomyByName(name);
        }
    }
}