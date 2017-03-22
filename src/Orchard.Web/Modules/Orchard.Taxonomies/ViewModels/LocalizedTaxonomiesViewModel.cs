using Orchard.Environment.Extensions;
using Orchard.Taxonomies.Settings;

namespace Orchard.Taxonomies.ViewModels {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomiesViewModel {
        public string ContentType { get; set; }
        public string FieldName { get; set; }
        public int Id { get; set; }
        public TaxonomyFieldSettings Setting { get; set; }
    }
}