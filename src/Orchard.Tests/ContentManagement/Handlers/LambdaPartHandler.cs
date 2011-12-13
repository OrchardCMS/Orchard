using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Tests.ContentManagement.Models;
using Orchard.Tests.ContentManagement.Records;

namespace Orchard.Tests.ContentManagement.Handlers {
    public class LambdaPartHandler : ContentHandler {
        public LambdaPartHandler(IRepository<LambdaRecord> repository) {
            Filters.Add(new ActivatingFilter<LambdaPart>("lambda"));
            Filters.Add(StorageFilter.For(repository));

        }
    }
}
