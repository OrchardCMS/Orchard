using Orchard.Data.Conventions;
using Orchard.Environment.Extensions;

namespace Orchard.Localization.Models {
    [OrchardFeature("Orchard.Localization.Transliteration")]
    public class TransliterationSpecificationRecord {
        public virtual int Id { get; set; }
        public virtual string CultureFrom { get; set; }
        public virtual string CultureTo { get; set; }

        [StringLengthMax]
        public virtual string Rules { get; set; }
    }
}