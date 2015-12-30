using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;

namespace Orchard.AntiSpam.Settings {
    public class SubmissionLimitPartSettingsEvents : ContentDefinitionEditorEventsBase {

        public Localizer T { get; set; }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "SubmissionLimitPart")
                yield break;

            var settings = definition.Settings.GetModel<SubmissionLimitPartSettings>();

            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "SubmissionLimitPart")
                yield break;

            var settings = new SubmissionLimitPartSettings {
            };

            if (updateModel.TryUpdateModel(settings, "SubmissionLimitPartSettings", null, null)) {

                builder.WithSetting("SubmissionLimitPartSettings.Limit", settings.Limit.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("SubmissionLimitPartSettings.Unit", settings.Unit.ToString(CultureInfo.InvariantCulture));
            }

            yield return DefinitionTemplate(settings);
        }
    }
}
