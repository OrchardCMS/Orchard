using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Fields.Settings {
    public class MediaPickerFieldEditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "MediaPickerField") {
                var model = definition.Settings.GetModel<MediaPickerFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "MediaPickerField") {
                yield break;
            }

            var model = new MediaPickerFieldSettings();
            if (updateModel.TryUpdateModel(model, "MediaPickerFieldSettings", null, null)) {
                builder.WithSetting("MediaPickerFieldSettings.Hint", model.Hint);
                builder.WithSetting("MediaPickerFieldSettings.AllowedExtensions", model.AllowedExtensions);
                builder.WithSetting("MediaPickerFieldSettings.Required", model.Required.ToString());
                builder.WithSetting("MediaPickerFieldSettings.Custom1", model.Custom1);
                builder.WithSetting("MediaPickerFieldSettings.Custom2", model.Custom2);
                builder.WithSetting("MediaPickerFieldSettings.Custom3", model.Custom3);
            }

            yield return DefinitionTemplate(model);
        }
    }
}