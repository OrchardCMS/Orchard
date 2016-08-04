using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.MediaLibrary.Settings {
    public class MediaLibraryPickerFieldEditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "MediaLibraryPickerField") {
                var model = definition.Settings.GetModel<MediaLibraryPickerFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "MediaLibraryPickerField") {
                yield break;
            }

            var model = new MediaLibraryPickerFieldSettings();
            if (updateModel.TryUpdateModel(model, "MediaLibraryPickerFieldSettings", null, null)) {
                builder.WithSetting("MediaLibraryPickerFieldSettings.Hint", model.Hint);
                builder.WithSetting("MediaLibraryPickerFieldSettings.Required", model.Required.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("MediaLibraryPickerFieldSettings.Multiple", model.Multiple.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("MediaLibraryPickerFieldSettings.DisplayedContentTypes", model.DisplayedContentTypes);
            }

            yield return DefinitionTemplate(model);
        }
    }
}