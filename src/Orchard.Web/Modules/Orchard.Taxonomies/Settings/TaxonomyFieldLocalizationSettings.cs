using Orchard.Environment.Extensions;

namespace Orchard.Taxonomies.Settings {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class TaxonomyFieldLocalizationSettings {

 //       public bool AssertSameCulture { get; set; }
        public bool TryToLocalize { get; set; }
        public bool RemoveItemsWithoutLocalization { get; set; }
    }
}