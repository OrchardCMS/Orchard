using System.ComponentModel.DataAnnotations;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.Records;

namespace Orchard.Wikis.Models {
    public class WikiSettingsRecord : ContentPartRecord {
        public virtual bool AllowAnonymousEdits { get; set; }

        [Required]
        public virtual string WikiEditTheme { get; set; }
    }

    public class WikiSettingsProvider : ContentProvider {
        public WikiSettingsProvider(IRepository<WikiSettingsRecord> repository) {
            Filters.Add(new ActivatingFilter<ContentPart<WikiSettingsRecord>>("site"));
            Filters.Add(new StorageFilter<WikiSettingsRecord>(repository) { AutomaticallyCreateMissingRecord = true });
            Filters.Add(new TemplateFilterForRecord<WikiSettingsRecord>("WikiSettings"));
        }
    }

}
