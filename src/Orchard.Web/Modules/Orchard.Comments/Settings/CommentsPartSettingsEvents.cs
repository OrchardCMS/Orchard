using System.Collections.Generic;
using System.Globalization;
using Orchard.Comments.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;
using Orchard.Services;

namespace Orchard.Comments.Settings {
    public class CommentsPartSettingsEvents : ContentDefinitionEditorEventsBase {
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;

        public CommentsPartSettingsEvents(IEnumerable<IHtmlFilter> htmlFilters) {
            _htmlFilters = htmlFilters;
        }

        public Localizer T { get; set; }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "CommentsPart")
                yield break;

            var model = new CommentsPartSettingsViewModel {
                Settings = definition.Settings.GetModel<CommentsPartSettings>(),
                HtmlFilters = _htmlFilters
            };

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "CommentsPart")
                yield break;

            var model = new CommentsPartSettingsViewModel {
                Settings = new CommentsPartSettings()
            };

            if (updateModel.TryUpdateModel(model, "CommentsPartSettingsViewModel", null, null)) {
                builder.WithSetting("CommentsPartSettings.DefaultThreadedComments", model.Settings.DefaultThreadedComments.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("CommentsPartSettings.MustBeAuthenticated", model.Settings.MustBeAuthenticated.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("CommentsPartSettings.HtmlFilter", model.Settings.HtmlFilter);
            }

            yield return DefinitionTemplate(model);
        }
    }
}
