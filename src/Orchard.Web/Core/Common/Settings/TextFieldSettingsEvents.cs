using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Core.Common.ViewModels;

namespace Orchard.Core.Common.Settings {
    public class TextFieldSettingsEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "TextField") {
                var model = new TextFieldSettingsEventsViewModel {
                    Settings = definition.Settings.GetModel<TextFieldSettings>(),
                };
                    
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "TextField") {
                yield break;
            }

            var model = new TextFieldSettingsEventsViewModel {
                Settings = new TextFieldSettings()
            };

            if (updateModel.TryUpdateModel(model, "TextFieldSettingsEventsViewModel", null, null)) {
                builder.WithSetting("TextFieldSettings.Flavor", model.Settings.Flavor);
                builder.WithSetting("TextFieldSettings.Hint", model.Settings.Hint);
                builder.WithSetting("TextFieldSettings.Required", model.Settings.Required.ToString(CultureInfo.InvariantCulture));

                yield return DefinitionTemplate(model);
            }
        }
    }
}