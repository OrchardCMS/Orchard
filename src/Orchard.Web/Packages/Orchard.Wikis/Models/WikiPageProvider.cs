using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Wikis.Models {
    public class WikiPageProvider : ContentProvider {
        public WikiPageProvider(IRepository<WikiPageRecord> wikiPageRepository) {
            Filters.Add(new ActivatingFilter<WikiPage>("wikipage"));
            Filters.Add(new ActivatingFilter<CommonPart>("wikipage"));
            Filters.Add(new ActivatingFilter<RoutablePart>("wikipage"));
            Filters.Add(new ActivatingFilter<BodyPart>("wikipage"));
            Filters.Add(new StorageFilter<WikiPageRecord>(wikiPageRepository) { AutomaticallyCreateMissingRecord = true });
        }
    }
}
