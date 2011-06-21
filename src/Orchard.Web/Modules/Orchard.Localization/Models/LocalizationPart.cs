using Orchard.ContentManagement;
using Orchard.Utility;
using Orchard.Localization.Records;

namespace Orchard.Localization.Models {
    public sealed class LocalizationPart : ContentPart<LocalizationPartRecord> {
        private readonly LazyField<CultureRecord> _culture = new LazyField<CultureRecord>();
        private readonly LazyField<IContent> _masterContentItem = new LazyField<IContent>();

        public LazyField<CultureRecord> CultureField { get { return _culture; } }
        public LazyField<IContent> MasterContentItemField { get { return _masterContentItem; } }

        public CultureRecord Culture {
            get { return _culture.Value; }
            set { _culture.Value = value; }
        }

        public IContent MasterContentItem {
            get { return _masterContentItem.Value; }
            set { _masterContentItem.Value = value; }
        }

        public bool HasTranslationGroup {
            get {
                return Record.MasterContentItemId != 0;
            }
        }
    }
}
