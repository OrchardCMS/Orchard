using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Utilities;
using Orchard.Localization.Records;

namespace Orchard.Localization.Models
{
    public sealed class LocalizationPart : ContentPart<LocalizationPartRecord>, ILocalizableAspect
    {
        public LazyField<CultureRecord> CultureField { get; } = new LazyField<CultureRecord>();
        public LazyField<IContent> MasterContentItemField { get; } = new LazyField<IContent>();

        public CultureRecord Culture
        {
            get { return CultureField.Value; }
            set { CultureField.Value = value; }
        }

        public IContent MasterContentItem
        {
            get { return MasterContentItemField.Value; }
            set { MasterContentItemField.Value = value; }
        }

        public bool HasTranslationGroup => Record.MasterContentItemId != 0;

        string ILocalizableAspect.Culture => Culture == null ? null : Culture.Culture;
    }
}
