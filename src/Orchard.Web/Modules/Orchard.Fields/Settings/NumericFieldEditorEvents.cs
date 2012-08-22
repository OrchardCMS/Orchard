using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Fields.Settings {
    public class NumericFieldListModeEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "NumericField") {
                var model = definition.Settings.GetModel<NumericFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "NumericField") {
                yield break;
            }

            var model = new NumericFieldSettings();
            if (updateModel.TryUpdateModel(model, "NumericFieldSettings", null, null)) {
                builder.WithSetting("NumericFieldSettings.Hint", model.Hint);
                builder.WithSetting("NumericFieldSettings.Required", model.Required.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("NumericFieldSettings.Scale", model.Scale.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("NumericFieldSettings.Minimum", model.Minimum.HasValue ? model.Minimum.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
                builder.WithSetting("NumericFieldSettings.Maximum", model.Maximum.HasValue ? model.Maximum.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            }

            yield return DefinitionTemplate(model);
        }
    }
}