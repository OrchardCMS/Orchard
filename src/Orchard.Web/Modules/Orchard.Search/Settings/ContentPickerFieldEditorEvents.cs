using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;

namespace Orchard.Search.Settings {
    [OrchardFeature("Orchard.Search.ContentPicker")]
    public class ContentPickerFieldEditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "ContentPickerField") {
                var model = definition.Settings.GetModel<ContentPickerSearchFieldSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "ContentPickerField") {
                yield break;
            }

            var model = new ContentPickerSearchFieldSettings();
            if (updateModel.TryUpdateModel(model, "ContentPickerSearchFieldSettings", null, null)) {
                builder.WithSetting("ContentPickerSearchFieldSettings.ShowSearchTab", model.ShowSearchTab.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("ContentPickerSearchFieldSettings.SearchIndex", model.SearchIndex);
                builder.WithSetting("ContentPickerSearchFieldSettings.DisplayedContentTypes", model.DisplayedContentTypes);
            }

            yield return DefinitionTemplate(model);
        }
    }
}