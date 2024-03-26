using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentPicker.Fields;
using Orchard.ContentPicker.ViewModels;
using Orchard.Environment.Extensions;

namespace Orchard.ContentPicker.Drivers {
    [OrchardFeature("Orchard.ContentPicker.LocalizationExtensions")]
    public class ContentPickerFieldLocalizationDriver : ContentFieldDriver<Fields.ContentPickerField> {
        private readonly IContentManager _contentManager;

        public ContentPickerFieldLocalizationDriver(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        private static string GetPrefix(Fields.ContentPickerField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(Fields.ContentPickerField field, ContentPart part) {
            return field.Name;
        }

        protected override DriverResult Editor(ContentPart part, Fields.ContentPickerField field, dynamic shapeHelper) {
            return ContentShape("Fields_ContentPickerLocalization_Edit", GetDifferentiator(field, part),
                () => {
                    var model = new ContentPickerFieldViewModel {
                        Field = field,
                        Part = part,
                        ContentItems = _contentManager.GetMany<ContentItem>(field.Ids, VersionOptions.Latest, QueryHints.Empty).ToList()
                    };

                    model.SelectedIds = string.Join(",", field.Ids);

                    return shapeHelper.EditorTemplate(TemplateName: "Fields/ContentPickerLocalization.Edit", Model: model, Prefix: GetPrefix(field, part));
                });
        }

        protected override DriverResult Editor(ContentPart part, ContentPickerField field, IUpdateModel updater, dynamic shapeHelper) {
            return Editor(part, field, shapeHelper);
        }
    }
}