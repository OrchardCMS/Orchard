using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Settings;
using Orchard.Taxonomies.ViewModels;

namespace Orchard.Taxonomies.Drivers {
    [OrchardFeature("Orchard.Taxonomies.LocalizationExtensions")]
    public class LocalizedTaxonomyFieldDriver : ContentFieldDriver<TaxonomyField> {
        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }
        private readonly IContentDefinitionManager _contentDefinitionManager;
        public LocalizedTaxonomyFieldDriver(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
        }

        protected override DriverResult Editor(ContentPart part, TaxonomyField field, dynamic shapeHelper) {
            return ContentShape("Fields_TaxonomyFieldList_Edit", GetDifferentiator(field, part), () => {
                var templateName = "Fields/TaxonomyFieldList";
                var taxonomySettings= new TaxonomyFieldSettings();
                  var contentDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
                    if (contentDefinition != null) {
                        var taxonomyField = contentDefinition.Parts.SelectMany(p => p.PartDefinition.Fields).Where(x => x.FieldDefinition.Name == "TaxonomyField" && x.Name == field.Name).FirstOrDefault();
                         if (taxonomyField != null) {
                             taxonomySettings = taxonomyField.Settings.GetModel<TaxonomyFieldSettings>();

                        }
                    }
               
                var viewModel = new LocalizedTaxonomiesViewModel {
                    ContentType = part.ContentItem.ContentType,
                    FieldName = field.Name,
                    Id = part.ContentItem.Id,
                    Setting = taxonomySettings
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