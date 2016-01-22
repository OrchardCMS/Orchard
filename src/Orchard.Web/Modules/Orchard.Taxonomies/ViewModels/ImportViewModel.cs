using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.ViewModels {
    public class ImportViewModel {
        public TaxonomyPart Taxonomy { get; set; }
        public string Terms { get; set; }
    }
}