using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Tests.ContentManagement.Models;
using Orchard.Tests.ContentManagement.Records;

namespace Orchard.Tests.ContentManagement.Handlers {
    class GammaPartHandler : ContentHandler {
        public GammaPartHandler(IRepository<GammaRecord> repository) {
            Filters.Add(new ActivatingFilter<GammaPart>("gamma"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
