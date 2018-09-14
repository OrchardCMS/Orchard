using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;

namespace Orchard.MediaLibrary.Settings {
    [OrchardFeature("Orchard.MediaLibrary.LocalizationExtensions")]
    public class MediaLibraryPickerFieldLocalizationEditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "MediaLibraryPickerField") {
                var model = definition.Settings.GetModel<MediaLibraryPickerFieldLocalizationSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "MediaLibraryPickerField") {
                yield break;
            }

            var model = new MediaLibraryPickerFieldLocalizationSettings();
            if (updateModel.TryUpdateModel(model, "MediaLibraryPickerFieldLocalizationSettings", null, null)) {
                builder.WithSetting("MediaLibraryPickerFieldLocalizationSettings.TryToLocalizeMedia", model.TryToLocalizeMedia.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("MediaLibraryPickerFieldLocalizationSettings.RemoveItemsWithoutLocalization", model.RemoveItemsWithoutLocalization.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("MediaLibraryPickerFieldLocalizationSettings.RemoveItemsWithNoLocalizationPart", model.RemoveItemsWithNoLocalizationPart.ToString(CultureInfo.InvariantCulture));
            }

            yield return DefinitionTemplate(model);
        }
    }
}