using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Tests.ContentManagement.Models;
using Orchard.Tests.ContentManagement.Records;

namespace Orchard.Tests.ContentManagement.Handlers {
    public class DeltaPartHandler : ContentHandler {
        public DeltaPartHandler(IRepository<DeltaRecord> repository) {
            Filters.Add(new ActivatingFilter<DeltaPart>("delta"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
