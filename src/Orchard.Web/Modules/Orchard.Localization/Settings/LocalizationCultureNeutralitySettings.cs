using Orchard.Environment.Extensions;

namespace Orchard.Localization.Settings {
    [OrchardFeature("Orchard.Localization.CultureNeutralPartsAndFields")]
    public class LocalizationCultureNeutralitySettings {
        public bool CultureNeutral { get; set; } //this setting applies to both parts and fields, and can be controlled during type definition
    }
}