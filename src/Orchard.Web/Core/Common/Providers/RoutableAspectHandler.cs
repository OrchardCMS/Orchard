using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Core.Common.Providers {
    public class RoutableAspectHandler : ContentHandler {
        public RoutableAspectHandler(IRepository<RoutableRecord> repository) {
            Filters.Add(new StorageFilter<RoutableRecord>(repository));
        }
    }
}