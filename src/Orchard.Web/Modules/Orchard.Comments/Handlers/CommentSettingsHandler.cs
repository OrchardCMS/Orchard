using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Comments.Handlers {
    [UsedImplicitly]
    public class CommentSettingsHandler : ContentHandler {
        public CommentSettingsHandler(IRepository<CommentSettingsRecord> repository) {
            Filters.Add(new ActivatingFilter<CommentSettings>("site"));
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new TemplateFilterForRecord<CommentSettingsRecord>("CommentSettings", "Parts/Comments.SiteSettings"));
        }
    }
}