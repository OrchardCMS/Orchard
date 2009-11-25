using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.Records;

namespace Orchard.Tags.Models {
    public class TagSettings : ContentPart<TagSettingsRecord> {
    }

    public class TagSettingsRecord : ContentPartRecord {
        public virtual bool EnableTagsOnPages { get; set; }
    }

    public class TagSettingsProvider : ContentProvider {
        public TagSettingsProvider(IRepository<TagSettingsRecord> repository) {
            Filters.Add(new ActivatingFilter<TagSettings>("site"));
            Filters.Add(new StorageFilter<TagSettingsRecord>(repository) { AutomaticallyCreateMissingRecord = true });
            Filters.Add(new TemplateFilterForRecord<TagSettingsRecord>("TagSettings"));
            AddOnActivated<TagSettings>(DefaultSettings);
        }

        private static void DefaultSettings(ActivatedContentContext context, TagSettings settings) {
            settings.Record.EnableTagsOnPages = true;
        }
    }
}
