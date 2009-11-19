using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.Records;
using Orchard.UI.Models;

namespace Orchard.Media.Models {
    public class MediaSettings : ContentPartForRecord<MediaSettingsRecord> {
    }

    public class MediaSettingsRecord : ContentPartRecord {
        public virtual string RootMediaFolder { get; set; }
    }

    public class MediaSettingsHandler : ContentHandler {
        public MediaSettingsHandler(IRepository<MediaSettingsRecord> repository) {
            Filters.Add(new ActivatingFilter<MediaSettings>("site"));
            Filters.Add(new StorageFilterForRecord<MediaSettingsRecord>(repository) { AutomaticallyCreateMissingRecord = true });
        }

        protected override void GetEditors(GetContentEditorsContext context) {
            var model = context.ContentItem.As<MediaSettings>();
            if (model == null)
                return;

            context.Editors.Add(ModelTemplate.For(model.Record, "MediaSettings"));
        }

        protected override void UpdateEditors(UpdateContentContext context) {
            var model = context.ContentItem.As<MediaSettings>();
            if (model == null)
                return;

            context.Updater.TryUpdateModel(model.Record, "MediaSettings", null, null);
            context.Editors.Add(ModelTemplate.For(model.Record, "MediaSettings"));
        }
    }
}

