using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Fields.Settings {
    public class DateTimeFieldEditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "DateTimeField") {
                var model = definition.Settings.GetModel<DateTimeFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "DateTimeField") {
                yield break;
            }

            var model = new DateTimeFieldSettings();
            if(updateModel.TryUpdateModel(model, "DateTimeFieldSettings", null, null)) {
                builder.WithSetting("DateTimeFieldSettings.Display", model.Display.ToString());
                builder.WithSetting("DateTimeFieldSettings.Hint", model.Hint);
                builder.WithSetting("DateTimeFieldSettings.Required", model.Required.ToString(CultureInfo.InvariantCulture));

                yield return DefinitionTemplate(model);
            }
        }
    }
}