using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Environment.Extensions;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Services {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class TaxonomyExtensionsService : ITaxonomyExtensionsService {
        private readonly IContentManager _contentManager;

        public TaxonomyExtensionsService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IEnumerable<ContentTypeDefinition> GetAllTermTypes() {
            return _contentManager.GetContentTypeDefinitions().Where(t => t.Parts.Any(p => p.PartDefinition.Name.Equals(typeof(TermPart).Name)));
        }
    }
}