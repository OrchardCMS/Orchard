using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;

namespace Orchard.AntiSpam.Settings {
    public class SpamFilterPartSettingsEvents : ContentDefinitionEditorEventsBase {

        public Localizer T { get; set; }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "SpamFilterPart")
                yield break;

            var settings = definition.Settings.GetModel<SpamFilterPartSettings>();

            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "SpamFilterPart")
                yield break;

            var settings = new SpamFilterPartSettings {
            };

            if (updateModel.TryUpdateModel(settings, "SpamFilterPartSettings", null, null)) {
                builder.WithSetting("SpamFilterPartSettings.Action", settings.Action.ToString());
                builder.WithSetting("SpamFilterPartSettings.Pattern", settings.Pattern);
            }

            yield return DefinitionTemplate(settings);
        }
    }
}
