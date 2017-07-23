using Orchard.Environment.Extensions;

namespace Orchard.Taxonomies.Settings {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class TaxonomyFieldLocalizationSettings {
        public bool TryToLocalize { get; set; }
        public TaxonomyFieldLocalizationSettings() {
            TryToLocalize = true; // default value
        }
    }
}