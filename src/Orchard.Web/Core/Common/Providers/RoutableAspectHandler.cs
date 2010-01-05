using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Common.Providers {
    public class RoutableAspectHandler : ContentHandler {
        public RoutableAspectHandler(IRepository<RoutableRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}