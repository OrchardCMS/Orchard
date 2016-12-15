using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.ViewModels;

namespace Orchard.Taxonomies.Drivers {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomyFieldDriver : ContentFieldDriver<TaxonomyField> {
        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }
        protected override DriverResult Editor(ContentPart part, TaxonomyField field, dynamic shapeHelper) {
            return ContentShape("Fields_TaxonomyFieldList_Edit", GetDifferentiator(field, part), () => {
                var templateName = "Fields/TaxonomyFieldList";
                var viewModel = new LocalizedTaxonomiesViewModel {
                    ContentType = part.ContentItem.ContentType,
                    FieldName = field.Name,
                    Id = part.ContentItem.Id
                };
                return shapeHelper.EditorTemplate(TemplateName: templateName, Model: viewModel, Prefix: GetPrefix(field, part));
            });
        }

        protected override DriverResult Editor(ContentPart part, TaxonomyField field, IUpdateModel updater, dynamic shapeHelper) {
            return Editor(part, field, shapeHelper);
        }

        private static string GetDifferentiator(TaxonomyField field, ContentPart part) {
            return field.Name;
        }

    }
}