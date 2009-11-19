using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Wikis.Models {
    public class WikiPageHandler : ContentHandler {
        public WikiPageHandler(IRepository<WikiPageRecord> wikiPageRepository) {
            Filters.Add(new ActivatingFilter<WikiPage>("wikipage"));
            Filters.Add(new ActivatingFilter<CommonPart>("wikipage"));
            Filters.Add(new ActivatingFilter<RoutablePart>("wikipage"));
            Filters.Add(new ActivatingFilter<ContentPart>("wikipage"));
            Filters.Add(new StorageFilterForRecord<WikiPageRecord>(wikiPageRepository) { AutomaticallyCreateMissingRecord = true });
        }
    }
}
