using System.Globalization;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Environment.Extensions;

namespace Orchard.Localization.Settings {
    [OrchardFeature("Orchard.Localization.CultureNeutralPartsAndFields")]
    public class LocalizationCultureNeutralitySettings {
        public bool CultureNeutral { get; set; } //this setting applies to bath parts and fields, and can be controlled during type definition
        public bool AllowSynchronization { get; set; } //this setting applies to parts only, and can be controlled during part definition
    }
}