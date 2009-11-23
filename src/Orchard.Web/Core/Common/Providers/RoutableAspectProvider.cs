using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Core.Common.Providers {
    public class RoutableAspectProvider : ContentProvider {
        public RoutableAspectProvider(IRepository<RoutableRecord> repository) {
            Filters.Add(new StorageFilter<RoutableRecord>(repository));
        }
    }
}