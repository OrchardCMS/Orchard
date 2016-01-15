using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Collections.Generic;
using System.Globalization;

namespace Orchard.Fields.Settings {
    public class LinkFieldListModeEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "LinkField") {
                var model = definition.Settings.GetModel<LinkFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "LinkField") {
                yield break;
            }
            
            var model = new LinkFieldSettings();
            if (updateModel.TryUpdateModel(model, "LinkFieldSettings", null, null)) {
                builder.WithSetting("LinkFieldSettings.Hint", model.Hint);
                builder.WithSetting("LinkFieldSettings.Required", model.Required.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("LinkFieldSettings.TargetMode", model.TargetMode.ToString());
                builder.WithSetting("LinkFieldSettings.LinkTextMode", model.LinkTextMode.ToString());
                builder.WithSetting("LinkFieldSettings.StaticText", model.StaticText);
                builder.WithSetting("LinkFieldSettings.DefaultValue", model.DefaultValue);
                builder.WithSetting("LinkFieldSettings.TextDefaultValue", model.TextDefaultValue);

                yield return DefinitionTemplate(model);
            }
        }
    }
}