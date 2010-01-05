using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;

namespace Orchard.Tags.Models {
    public class TagSettings : ContentPart<TagSettingsRecord> {
    }

    public class TagSettingsRecord : ContentPartRecord {
        public virtual bool EnableTagsOnPages { get; set; }
    }

    public class TagSettingsHandler : ContentHandler {
        public TagSettingsHandler(IRepository<TagSettingsRecord> repository) {
            Filters.Add(new ActivatingFilter<TagSettings>("site"));
            Filters.Add(new StorageFilter<TagSettingsRecord>(repository));
            Filters.Add(new TemplateFilterForRecord<TagSettingsRecord>("TagSettings", "Parts/Tags.SiteSettings"));
            OnActivated<TagSettings>(DefaultSettings);
        }

        private static void DefaultSettings(ActivatedContentContext context, TagSettings settings) {
            settings.Record.EnableTagsOnPages = true;
        }
    }
}
