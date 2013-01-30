using Orchard.ContentManagement.Records;

namespace Contrib.Taxonomies.Models {
    public class TaxonomyPartRecord : ContentPartRecord {
        public virtual string TermTypeName { get; set; }
    }
}
