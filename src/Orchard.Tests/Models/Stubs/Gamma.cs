using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Models.Records;

namespace Orchard.Tests.Models.Stubs {
    public class Gamma : ContentItemPartWithRecord<GammaRecord> {
    }

    public class GammaRecord : ContentPartRecordBase {
        public virtual string Frap { get; set; }
    }


    public class GammaDriver : ModelDriver {
        public GammaDriver(IRepository<GammaRecord> repository){
            Filters.Add(new ActivatingFilter<Gamma>(x => x == "gamma"));
            Filters.Add(new StorageFilterForRecord<GammaRecord>(repository));
        }
    }
}
