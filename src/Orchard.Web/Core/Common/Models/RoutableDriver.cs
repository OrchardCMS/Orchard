using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Core.Common.Models {
    public class RoutableDriver : Orchard.Models.Driver.ContentHandler {
        public RoutableDriver(IRepository<RoutableRecord> repository) {
            Filters.Add(new StorageFilterForRecord<RoutableRecord>(repository));
        }
    }
}