using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Tests.ContentManagement.Records;

namespace Orchard.Tests.ContentManagement.Models {
    public class Delta : ContentPart<DeltaRecord> {
    }


    public class DeltaHandler : ContentHandler {

        public DeltaHandler(IRepository<DeltaRecord> repository) {
            Filters.Add(new ActivatingFilter<Delta>(x => x == "delta"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
