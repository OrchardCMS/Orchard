using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Tests.Models.Records;

namespace Orchard.Tests.Models.Stubs {
    public class Gamma : ContentPart<GammaRecord> {
    }


    public class GammaHandler : ContentHandler {
        public override System.Collections.Generic.IEnumerable<ContentType> GetContentTypes() {
            return new[] { new ContentType { Name = "gamma" } };
        }

        public GammaHandler(IRepository<GammaRecord> repository) {
            Filters.Add(new ActivatingFilter<Gamma>(x => x == "gamma"));
            Filters.Add(new StorageFilter<GammaRecord>(repository));
        }
    }


}
