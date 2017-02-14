using Orchard.Environment.Extensions;

namespace Orchard.Taxonomies.ViewModels {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomiesViewModel {
        public string ContentType { get; set; }
        public string FieldName { get; set; }
        public int Id { get; set; }
    }
}