using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.Services {
    public interface ITaxonomySource : IDependency {
        TaxonomyPart GetTaxonomy(string name, ContentItem item);
    }
}