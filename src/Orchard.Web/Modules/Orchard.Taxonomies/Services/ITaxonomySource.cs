using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Services {
    public interface ITaxonomySource : IDependency {
        //IEnumerable<TermPart> GetAppliedTerms(ContentPart part, TaxonomyField field = null, VersionOptions versionOptions = null);
        TaxonomyPart GetTaxonomy(string name, ContentItem item);
    }
}