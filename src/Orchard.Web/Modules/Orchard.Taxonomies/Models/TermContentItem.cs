using Orchard.Data.Conventions;

namespace Orchard.Taxonomies.Models {
    /// <summary>
    /// Represents a relationship between a Term and a Content Item
    /// </summary>
    public class TermContentItem {

        public virtual int Id { get; set; }
        public virtual string Field { get; set; }
        public virtual TermPartRecord TermRecord { get; set; }

        [CascadeAllDeleteOrphan]
        public virtual TermsPartRecord TermsPartRecord { get; set; }
    }
}