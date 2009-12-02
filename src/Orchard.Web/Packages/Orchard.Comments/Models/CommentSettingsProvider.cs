using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Comments.Models {
    public class CommentSettingsProvider : ContentProvider {
        private readonly IRepository<CommentSettingsRecord> _commentSettingsRepository;

        public CommentSettingsProvider(IRepository<CommentSettingsRecord> repository) {
            _commentSettingsRepository = repository;
            Filters.Add(new ActivatingFilter<CommentSettings>("site"));
            Filters.Add(new StorageFilter<CommentSettingsRecord>(_commentSettingsRepository) { AutomaticallyCreateMissingRecord = true });
            Filters.Add(new TemplateFilterForRecord<CommentSettingsRecord>("CommentSettings"));
            OnActivated<CommentSettings>(DefaultSettings);
        }

        private static void DefaultSettings(ActivatedContentContext context, CommentSettings settings) {
            settings.Record.EnableCommentsOnPages = true;
            settings.Record.EnableCommentsOnPosts = true;
        }
    }
}