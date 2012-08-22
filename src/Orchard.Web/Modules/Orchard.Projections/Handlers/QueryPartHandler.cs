using System.Linq;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Projections.Models;

namespace Orchard.Projections.Handlers {
    public class QueryPartHandler : ContentHandler {

        public QueryPartHandler(IRepository<QueryPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));

            T = NullLocalizer.Instance;

            // create a default FilterGroup on creation
            OnPublishing<QueryPart>(CreateFilterGroup);

        }

        public Localizer T { get; set; }

        private static void CreateFilterGroup(PublishContentContext ctx, QueryPart part) {
            if (!part.FilterGroups.Any()) {
                part.FilterGroups.Add(new FilterGroupRecord());
            }
        }
    }
}