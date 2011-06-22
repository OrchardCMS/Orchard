using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Metadata;
using Orchard.ContentManagement.Metadata.Builders;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Core.Common.DateEditor {
    public class DateEditorSettings {
        public bool ShowDateEditor { get; set; }
    }

    public class DateEditorSettingsEvents : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name == "CommonPart") {
                var model = definition.Settings.GetModel<DateEditorSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name == "CommonPart") {
                var model = new DateEditorSettings();
                if (updateModel.TryUpdateModel(model, "DateEditorSettings", null, null)) {
                    builder.WithSetting("DateEditorSettings.ShowDateEditor", model.ShowDateEditor.ToString());
                }
                yield return DefinitionTemplate(model);
            }
        }
    }
}