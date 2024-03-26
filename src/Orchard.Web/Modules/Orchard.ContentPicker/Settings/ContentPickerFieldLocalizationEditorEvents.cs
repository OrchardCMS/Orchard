using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;

namespace Orchard.ContentPicker.Settings {
    [OrchardFeature("Orchard.ContentPicker.LocalizationExtensions")]
    public class ContentPickerFieldLocalizationEditorEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "ContentPickerField") {
                var model = definition.Settings.GetModel<ContentPickerFieldLocalizationSettings>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "ContentPickerField") {
                yield break;
            }

            var model = new ContentPickerFieldLocalizationSettings();
            if (updateModel.TryUpdateModel(model, "ContentPickerFieldLocalizationSettings", null, null)) {
                builder.WithSetting("ContentPickerFieldLocalizationSettings.TryToLocalizeItems", model.TryToLocalizeItems.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("ContentPickerFieldLocalizationSettings.RemoveItemsWithoutLocalization", model.RemoveItemsWithoutLocalization.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("ContentPickerFieldLocalizationSettings.RemoveItemsWithNoLocalizationPart", model.RemoveItemsWithNoLocalizationPart.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("ContentPickerFieldLocalizationSettings.AssertItemsHaveSameCulture", model.AssertItemsHaveSameCulture.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("ContentPickerFieldLocalizationSettings.BlockForItemsWithNoLocalizationPart", model.BlockForItemsWithNoLocalizationPart.ToString(CultureInfo.InvariantCulture));
            }

            yield return DefinitionTemplate(model);
        }
    }
}