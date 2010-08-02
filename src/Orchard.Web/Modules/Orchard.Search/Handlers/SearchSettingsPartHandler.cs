using JetBrains.Annotations;
using Orchard.Search.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Search.Handlers {
    [UsedImplicitly]
    public class SearchSettingsPartHandler : ContentHandler {
        public SearchSettingsPartHandler(IRepository<SearchSettingsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<SearchSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new TemplateFilterForRecord<SearchSettingsPartRecord>("CommentSettings", "Parts/Search.SiteSettings"));
        }
    }
}