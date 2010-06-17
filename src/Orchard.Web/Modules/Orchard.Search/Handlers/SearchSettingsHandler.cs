using JetBrains.Annotations;
using Orchard.Search.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Search.Handlers {
    [UsedImplicitly]
    public class SearchSettingsHandler : ContentHandler {
        public SearchSettingsHandler(IRepository<SearchSettingsRecord> repository) {
            Filters.Add(new ActivatingFilter<SearchSettings>("site"));
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new TemplateFilterForRecord<SearchSettingsRecord>("CommentSettings", "Parts/Search.SiteSettings"));
        }
    }
}