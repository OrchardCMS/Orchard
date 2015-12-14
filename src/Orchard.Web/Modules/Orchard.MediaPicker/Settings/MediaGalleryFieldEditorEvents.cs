using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.MediaPicker.Settings {
    public class MediaGalleryFieldEditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "MediaGalleryField") {
                var model = definition.Settings.GetModel<MediaGalleryFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "MediaGalleryField") {
                yield break;
            }

            var model = new MediaGalleryFieldSettings();
            if (updateModel.TryUpdateModel(model, "MediaGalleryFieldSettings", null, null)) {
                builder.WithSetting("MediaGalleryFieldSettings.Hint", model.Hint);
                builder.WithSetting("MediaGalleryFieldSettings.AllowedExtensions", model.AllowedExtensions);
                builder.WithSetting("MediaGalleryFieldSettings.Required", model.Required.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("MediaGalleryFieldSettings.Multiple", model.Multiple.ToString(CultureInfo.InvariantCulture));
            }

            yield return DefinitionTemplate(model);
        }
    }
}