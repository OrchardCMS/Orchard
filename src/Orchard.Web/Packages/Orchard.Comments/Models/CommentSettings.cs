using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.Records;

namespace Orchard.Comments.Models {
    public class CommentSettings : ContentPart<CommentSettingsRecord> {
    }

    public class CommentSettingsRecord : ContentPartRecord {
        public virtual bool RequireLoginToAddComment { get; set; }
        public virtual bool EnableCommentsOnPages { get; set; }
        public virtual bool EnableCommentsOnPosts { get; set; }
        public virtual bool EnableSpamProtection { get; set; }
        public virtual string AkismetKey { get; set; }
        public virtual string AkismetUrl { get; set; }
    }

    public class CommentSettingsProvider : ContentProvider {
        private readonly IRepository<CommentSettingsRecord> _commentSettingsRepository;
        public CommentSettingsProvider(IRepository<CommentSettingsRecord> repository) {
            _commentSettingsRepository = repository;
            Filters.Add(new ActivatingFilter<CommentSettings>("site"));
            Filters.Add(new StorageFilter<CommentSettingsRecord>(_commentSettingsRepository) { AutomaticallyCreateMissingRecord = true });
            Filters.Add(new TemplateFilterForRecord<CommentSettingsRecord>("CommentSettings"));
            AddOnActivated<CommentSettings>(DefaultSettings);
        }

        private static void DefaultSettings(ActivatedContentContext context, CommentSettings settings) {
            settings.Record.EnableCommentsOnPages = true;
            settings.Record.EnableCommentsOnPosts = true;
        }
    }
}
