using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.ViewModels;
using Orchard.Data;
using Orchard.Media.Models;

namespace Orchard.Media.Handlers {
    [UsedImplicitly]
    public class MediaSettingsHandler : ContentHandler {
        public MediaSettingsHandler(IRepository<MediaSettingsRecord> repository) {
            Filters.Add(new ActivatingFilter<MediaSettings>("site"));
            Filters.Add(StorageFilter.For(repository) );
            OnActivated<MediaSettings>(DefaultSettings);
        }

        private static void DefaultSettings(ActivatedContentContext context, MediaSettings settings) {
            settings.Record.RootMediaFolder = "~/Media";
        }

        protected override void BuildEditorModel(BuildEditorModelContext context) {
            var model = context.ContentItem.As<MediaSettings>();
            if (model == null)
                return;

            context.AddEditor(new TemplateViewModel(model.Record, "MediaSettings") { TemplateName = "Parts/Media.SiteSettings" });
        }

        protected override void UpdateEditorModel(UpdateEditorModelContext context) {
            var model = context.ContentItem.As<MediaSettings>();
            if (model == null)
                return;

            context.Updater.TryUpdateModel(model.Record, "MediaSettings", null, null);
            context.AddEditor(new TemplateViewModel(model.Record, "MediaSettings") { TemplateName = "Parts/Media.SiteSettings" });
        }
    }
}