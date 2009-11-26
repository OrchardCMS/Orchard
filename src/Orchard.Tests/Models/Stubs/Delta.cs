using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Data;
using Orchard.Models;
using Orchard.Models.Driver;
using Orchard.Tests.Models.Records;

namespace Orchard.Tests.Models.Stubs {
    public class Delta : ContentPart<DeltaRecord> {
    }


    public class DeltaProvider : ContentProvider {
        public DeltaProvider(IRepository<DeltaRecord> repository) {
            Filters.Add(new ActivatingFilter<Delta>(x => x == "delta"));
            Filters.Add(new StorageFilter<DeltaRecord>(repository));
        }
    }
}
