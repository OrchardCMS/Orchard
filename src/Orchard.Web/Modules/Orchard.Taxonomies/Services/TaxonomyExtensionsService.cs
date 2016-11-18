using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Environment.Extensions;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Services {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class TaxonomyExtensionsService : ITaxonomyExtensionsService {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITaxonomyService _taxonomyService;

        public TaxonomyExtensionsService(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager, ITaxonomyService taxonomyService) {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _taxonomyService = taxonomyService;
        }

        public IEnumerable<ContentTypeDefinition> GetAllTermTypes() {
            return _contentManager.GetContentTypeDefinitions().Where(t => t.Parts.Any(p => p.PartDefinition.Name.Equals(typeof(TermPart).Name)));
        }

        public void CreateLocalizedTermContentType(TaxonomyPart taxonomy) {
            _taxonomyService.CreateTermContentType(taxonomy);
            _contentDefinitionManager.AlterTypeDefinition(taxonomy.TermTypeName,
                cfg => cfg
                    .WithPart("LocalizationPart")
                );
        }
    }
}