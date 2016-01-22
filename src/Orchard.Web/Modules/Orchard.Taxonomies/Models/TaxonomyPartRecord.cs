using Orchard.ContentManagement.Records;

namespace Orchard.Taxonomies.Models {
    public class TaxonomyPartRecord : ContentPartRecord {
        public virtual string TermTypeName { get; set; }
        public virtual bool IsInternal { get; set; }
    }
}
