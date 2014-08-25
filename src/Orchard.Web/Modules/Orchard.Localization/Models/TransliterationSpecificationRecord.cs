using Orchard.Data.Conventions;

namespace Orchard.Localization.Models {
    public class TransliterationSpecificationRecord {
        public virtual int Id { get; set; }
        public virtual int LCIDFrom { get; set; }
        public virtual int LCIDTo { get; set; }

        [StringLengthMax]
        public virtual string Rules { get; set; }
    }
}