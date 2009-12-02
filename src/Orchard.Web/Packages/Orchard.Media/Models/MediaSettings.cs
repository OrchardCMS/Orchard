using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.Records;
using Orchard.Models.ViewModels;

namespace Orchard.Media.Models {
    public class MediaSettings : ContentPart<MediaSettingsRecord> {
    }

    public class MediaSettingsRecord : ContentPartRecord {
        public virtual string RootMediaFolder { get; set; }
    }

    public class MediaSettingsProvider : ContentProvider {
        public MediaSettingsProvider(IRepository<MediaSettingsRecord> repository) {
            Filters.Add(new ActivatingFilter<MediaSettings>("site"));
            Filters.Add(new StorageFilter<MediaSettingsRecord>(repository) { AutomaticallyCreateMissingRecord = true });
            OnActivated<MediaSettings>(DefaultSettings);
        }

        private static void DefaultSettings(ActivatedContentContext context, MediaSettings settings) {
            settings.Record.RootMediaFolder = "~/Media";
        }

        protected override void GetEditors(GetEditorsContext context) {
            var model = context.ContentItem.As<MediaSettings>();
            if (model == null)
                return;

            context.AddEditor(new TemplateViewModel(model.Record, "MediaSettings"));
        }

        protected override void UpdateEditors(UpdateContentContext context) {
            var model = context.ContentItem.As<MediaSettings>();
            if (model == null)
                return;

            context.Updater.TryUpdateModel(model.Record, "MediaSettings", null, null);
            context.AddEditor(new TemplateViewModel(model.Record, "MediaSettings"));
        }
    }
}

