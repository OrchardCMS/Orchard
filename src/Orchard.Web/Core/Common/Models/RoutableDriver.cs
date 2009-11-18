using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.Models.Driver;

namespace Orchard.Core.Common.Models {
    public class RoutableDriver : ModelDriverWithRecord<RoutableRecord> {
        public RoutableDriver(IRepository<RoutableRecord> repository) : base(repository) {
        }
    }
}