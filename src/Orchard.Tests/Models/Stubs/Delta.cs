using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Tests.Models.Records;

namespace Orchard.Tests.Models.Stubs {
    public class Delta : ContentPart<DeltaRecord> {
    }


    public class DeltaHandler : ContentHandler {
        public override System.Collections.Generic.IEnumerable<Orchard.ContentManagement.ContentType> GetContentTypes() {
            return new[] { new ContentType { Name = "delta" } };
        }

        public DeltaHandler(IRepository<DeltaRecord> repository) {
            Filters.Add(new ActivatingFilter<Delta>(x => x == "delta"));
            Filters.Add(new StorageFilter<DeltaRecord>(repository));
        }
    }
}
