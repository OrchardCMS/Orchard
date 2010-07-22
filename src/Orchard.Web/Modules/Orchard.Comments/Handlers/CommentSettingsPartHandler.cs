using JetBrains.Annotations;
using Orchard.Comments.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Comments.Handlers {
    [UsedImplicitly]
    public class CommentSettingsPartHandler : ContentHandler {
        public CommentSettingsPartHandler(IRepository<CommentSettingsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<CommentSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new TemplateFilterForRecord<CommentSettingsPartRecord>("CommentSettingsPart", "Parts/Comments.SiteSettings"));
        }
    }
}