using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Core.Common.OwnerEditor {
    public class OwnerEditorSettings {

        public OwnerEditorSettings() {
            // owner editor should is displayed by default
            ShowOwnerEditor = true;
        }

        public bool ShowOwnerEditor { get; set; }
    }

    public class OwnerEditorSettingsEvents : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name == "CommonPart") {
                var model = definition.Settings.GetModel<OwnerEditorSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel upOwnerModel) {
            if (builder.Name == "CommonPart") {
                var model = new OwnerEditorSettings();
                if (upOwnerModel.TryUpdateModel(model, "OwnerEditorSettings", null, null)) {
                    builder.WithSetting("OwnerEditorSettings.ShowOwnerEditor", model.ShowOwnerEditor.ToString());
                }
                yield return DefinitionTemplate(model);
            }
        }
    }
}