using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.MediaLibrary.MediaFileName {
    public class MediaFileNameEditorSettings {

        public MediaFileNameEditorSettings() {
            // file name editor should not be displayed by default
            ShowFileNameEditor = false;
        }

        public bool ShowFileNameEditor { get; set; }
    }

    public class MediaFileNameEditorSettingsEvents : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition) {
            if (definition.Settings.ContainsKey("Stereotype") && definition.Settings["Stereotype"] == "Media") {
                var model = definition.Settings.GetModel<MediaFileNameEditorSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel) {
            var settings = builder.Current.Settings;
            if (settings.ContainsKey("Stereotype") && settings["Stereotype"] == "Media") {
                var model = new MediaFileNameEditorSettings();
                if (updateModel.TryUpdateModel(model, "MediaFileNameEditorSettings", null, null)) {
                    builder.WithSetting("MediaFileNameEditorSettings.ShowFileNameEditor", model.ShowFileNameEditor.ToString());
                }
            }
            return base.TypeEditorUpdate(builder, updateModel);
        }
    }
}