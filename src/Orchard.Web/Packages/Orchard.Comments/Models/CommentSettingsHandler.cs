using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Comments.Models {
    public class CommentSettingsHandler : ContentHandler {
        private readonly IRepository<CommentSettingsRecord> _commentSettingsRepository;

        public CommentSettingsHandler(IRepository<CommentSettingsRecord> repository) {
            _commentSettingsRepository = repository;
            Filters.Add(new ActivatingFilter<CommentSettings>("site"));
            Filters.Add(new StorageFilter<CommentSettingsRecord>(_commentSettingsRepository) { AutomaticallyCreateMissingRecord = true });
            Filters.Add(new TemplateFilterForRecord<CommentSettingsRecord>("CommentSettings", "Parts/Comments.SiteSettings"));
            OnActivated<CommentSettings>(DefaultSettings);
        }

        private static void DefaultSettings(ActivatedContentContext context, CommentSettings settings) {
            settings.Record.EnableCommentsOnPages = true;
            settings.Record.EnableCommentsOnPosts = true;
        }
    }
}