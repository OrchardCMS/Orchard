using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Tests.ContentManagement.Models;

namespace Orchard.Tests.ContentManagement.Handlers {
    public class EpsilonPartHandler : ContentHandler {
        public EpsilonPartHandler(IRepository<EpsilonRecord> repository) {
            Filters.Add(new ActivatingFilter<EpsilonPart>(x => x == "gamma"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
