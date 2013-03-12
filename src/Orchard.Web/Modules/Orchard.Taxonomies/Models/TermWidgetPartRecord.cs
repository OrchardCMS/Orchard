using Orchard.ContentManagement.Records;

namespace Orchard.Taxonomies.Models {
    public class TermWidgetPartRecord : ContentPartRecord {
        public TermWidgetPartRecord() {
            Count = 10;
        }

        public virtual TaxonomyPartRecord TaxonomyPartRecord { get; set; }
        public virtual TermPartRecord TermPartRecord { get; set; }
        
        public virtual int Count { get; set; }
        public virtual string OrderBy { get; set; }

        public virtual string FieldName { get; set; }
        public virtual string ContentType { get; set; }
    }
}