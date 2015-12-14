using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Fields.Settings {
    public class InputFieldListModeEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "InputField") {
                var model = definition.Settings.GetModel<InputFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "InputField") {
                yield break;
            }

            var model = new InputFieldSettings();
            if (updateModel.TryUpdateModel(model, "InputFieldSettings", null, null)) {
                builder.WithSetting("InputFieldSettings.Type", model.Type.ToString());
                builder.WithSetting("InputFieldSettings.Title", model.Title);
                builder.WithSetting("InputFieldSettings.Hint", model.Hint);
                builder.WithSetting("InputFieldSettings.Required", model.Required.ToString());
                builder.WithSetting("InputFieldSettings.AutoFocus", model.AutoFocus.ToString());
                builder.WithSetting("InputFieldSettings.AutoComplete", model.AutoComplete.ToString());
                builder.WithSetting("InputFieldSettings.Placeholder", model.Placeholder);
                builder.WithSetting("InputFieldSettings.Pattern", model.Pattern);
                builder.WithSetting("InputFieldSettings.EditorCssClass", model.EditorCssClass);
                builder.WithSetting("InputFieldSettings.MaxLength", model.MaxLength.ToString());
            }

            yield return DefinitionTemplate(model);
        }
    }
}