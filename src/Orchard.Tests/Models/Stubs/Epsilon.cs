using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Tests.Models.Records;

namespace Orchard.Tests.Models.Stubs {


    public class Epsilon : ContentPart<EpsilonRecord> {
    }

    public class EpsilonHandler : ContentHandler {

        public EpsilonHandler(IRepository<EpsilonRecord> repository) {
            Filters.Add(new ActivatingFilter<Epsilon>(x => x == "gamma"));
            Filters.Add(new StorageVersionFilter<EpsilonRecord>(repository));            
        }
    }
}
