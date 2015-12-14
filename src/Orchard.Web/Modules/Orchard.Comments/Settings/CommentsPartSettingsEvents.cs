using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;

namespace Orchard.Comments.Settings {
    public class CommentsPartSettingsEvents : ContentDefinitionEditorEventsBase {

        public Localizer T { get; set; }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "CommentsPart")
                yield break;

            var settings = definition.Settings.GetModel<CommentsPartSettings>();

            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "CommentsPart")
                yield break;

            var settings = new CommentsPartSettings {
            };

            if (updateModel.TryUpdateModel(settings, "CommentsPartSettings", null, null)) {
                builder.WithSetting("CommentsPartSettings.DefaultThreadedComments", settings.DefaultThreadedComments.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("CommentsPartSettings.MustBeAuthenticated", settings.MustBeAuthenticated.ToString(CultureInfo.InvariantCulture));
            }

            yield return DefinitionTemplate(settings);
        }
    }
}
