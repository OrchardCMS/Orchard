using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Comments.Models {
    public class CommentSettingsHandler : ContentHandler {
        private readonly IRepository<CommentSettingsRecord> _commentSettingsRepository;

        public CommentSettingsHandler(IRepository<CommentSettingsRecord> repository) {
            _commentSettingsRepository = repository;

            Filters.Add(new ActivatingFilter<CommentSettings>("site"));
            Filters.Add(StorageFilter.For(_commentSettingsRepository));
            Filters.Add(new TemplateFilterForRecord<CommentSettingsRecord>("CommentSettings", "Parts/Comments.SiteSettings"));
        }
    }
}