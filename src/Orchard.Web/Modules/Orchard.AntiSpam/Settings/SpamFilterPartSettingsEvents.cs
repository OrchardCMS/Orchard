using System.Collections.Generic;
using System.Globalization;
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
                builder.WithSetting("SpamFilterPartSettings.DeleteSpam", settings.DeleteSpam.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("SpamFilterPartSettings.UrlPattern", settings.UrlPattern);
                builder.WithSetting("SpamFilterPartSettings.PermalinkPattern", settings.PermalinkPattern);
                builder.WithSetting("SpamFilterPartSettings.CommentAuthorPattern", settings.CommentAuthorPattern);
                builder.WithSetting("SpamFilterPartSettings.CommentAuthorUrlPattern", settings.CommentAuthorUrlPattern);
                builder.WithSetting("SpamFilterPartSettings.CommentAuthorEmailPattern", settings.CommentAuthorEmailPattern);
                builder.WithSetting("SpamFilterPartSettings.CommentContentPattern", settings.CommentContentPattern);
            }

            yield return DefinitionTemplate(settings);
        }
    }
}
