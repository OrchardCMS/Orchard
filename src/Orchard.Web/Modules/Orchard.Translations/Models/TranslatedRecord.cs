using Orchard.Localization.Records;

namespace Orchard.Translations.Models {
    public class TranslatedRecord {
        public virtual int Id { get; set; }
        public virtual TranslatableRecord Parent { get; set; }
        public virtual CultureRecord Culture { get; set; }
        public virtual string Value { get; set; }
    }
}