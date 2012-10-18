using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Fields.Settings {
    public class BooleanFieldListModeEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "BooleanField") {
                var model = definition.Settings.GetModel<BooleanFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "BooleanField") {
                yield break;
            }

            var model = new BooleanFieldSettings();
            if (updateModel.TryUpdateModel(model, "BooleanFieldSettings", null, null)) {
                builder.WithSetting("BooleanFieldSettings.Hint", model.Hint);
                builder.WithSetting("BooleanFieldSettings.Optional", model.Optional.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("BooleanFieldSettings.NotSetLabel", model.NotSetLabel);
                builder.WithSetting("BooleanFieldSettings.OnLabel", model.OnLabel);
                builder.WithSetting("BooleanFieldSettings.OffLabel", model.OffLabel);
                builder.WithSetting("BooleanFieldSettings.SelectionMode", model.SelectionMode.ToString());
                builder.WithSetting("BooleanFieldSettings.DefaultValue", model.DefaultValue.HasValue ? model.DefaultValue.Value.ToString(CultureInfo.InvariantCulture) : string.Empty );
            }

            yield return DefinitionTemplate(model);
        }
    }
}