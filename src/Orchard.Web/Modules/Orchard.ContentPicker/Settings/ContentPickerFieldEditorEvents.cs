using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.ContentPicker.Settings {
    public class ContentPickerFieldEditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "ContentPickerField") {
                var model = definition.Settings.GetModel<ContentPickerFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "ContentPickerField") {
                yield break;
            }

            var model = new ContentPickerFieldSettings();
            if (updateModel.TryUpdateModel(model, "ContentPickerFieldSettings", null, null)) {
                builder.WithSetting("ContentPickerFieldSettings.Hint", model.Hint);
                builder.WithSetting("ContentPickerFieldSettings.Required", model.Required.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("ContentPickerFieldSettings.Multiple", model.Multiple.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("ContentPickerFieldSettings.ShowContentTab", model.ShowContentTab.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("ContentPickerFieldSettings.DisplayedContentTypes", model.DisplayedContentTypes);
            }

            yield return DefinitionTemplate(model);
        }
    }
}