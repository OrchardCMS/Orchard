using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Core.Common.Models {
    public class RoutablePartProvider : ContentProvider {
        public RoutablePartProvider(IRepository<RoutableRecord> repository) {
            Filters.Add(new StorageFilter<RoutableRecord>(repository));
        }
    }
}