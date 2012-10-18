using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Fields.Settings {
    public class EnumerationFieldListModeEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "EnumerationField") {
                var model = definition.Settings.GetModel<EnumerationFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "EnumerationField") {
                yield break;
            }

            var model = new EnumerationFieldSettings();
            if (updateModel.TryUpdateModel(model, "EnumerationFieldSettings", null, null)) {
                builder.WithSetting("EnumerationFieldSettings.Hint", model.Hint);
                builder.WithSetting("EnumerationFieldSettings.Required", model.Required.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("EnumerationFieldSettings.Options", model.Options);
                builder.WithSetting("EnumerationFieldSettings.ListMode", model.ListMode.ToString());
            }

            yield return DefinitionTemplate(model);
        }
    }
}