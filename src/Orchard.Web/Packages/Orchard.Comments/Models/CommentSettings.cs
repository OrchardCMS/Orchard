using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.Records;

namespace Orchard.Comments.Models {
    public class CommentSettings : ContentPartForRecord<CommentSettingsRecord> {
    }

    public class CommentSettingsRecord : ContentPartRecord {
        public virtual bool RequireLoginToAddComment { get; set; }
        public virtual bool EnableCommentsOnPages { get; set; }
        public virtual bool EnableCommentsOnPosts { get; set; }
        public virtual bool EnableSpamProtection { get; set; }
        public virtual string AkismetKey { get; set; }
        public virtual string AkismetUrl { get; set; }
    }

    public class CommentSettingsHandler : ContentHandler {
        public CommentSettingsHandler(IRepository<CommentSettingsRecord> repository) {
            Filters.Add(new ActivatingFilter<CommentSettings>("site"));
            Filters.Add(new StorageFilterForRecord<CommentSettingsRecord>(repository) { AutomaticallyCreateMissingRecord = true });
            Filters.Add(new TemplateFilterForRecord<CommentSettingsRecord>("CommentSettings"));
        }
    }
}
