using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement.ViewModels;

namespace Orchard.Media.Models {
    public class MediaSettings : ContentPart<MediaSettingsRecord> {
    }

    public class MediaSettingsRecord : ContentPartRecord {
        public virtual string RootMediaFolder { get; set; }
    }

    public class MediaSettingsHandler : ContentHandler {
        public MediaSettingsHandler(IRepository<MediaSettingsRecord> repository) {
            Filters.Add(new ActivatingFilter<MediaSettings>("site"));
            Filters.Add(new StorageFilter<MediaSettingsRecord>(repository) { AutomaticallyCreateMissingRecord = true });
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

