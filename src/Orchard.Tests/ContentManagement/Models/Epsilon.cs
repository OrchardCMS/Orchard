using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.Tests.ContentManagement.Models {


    public class Epsilon : ContentPart<EpsilonRecord> {
    }

    public class EpsilonHandler : ContentHandler {

        public EpsilonHandler(IRepository<EpsilonRecord> repository) {
            Filters.Add(new ActivatingFilter<Epsilon>(x => x == "gamma"));
            Filters.Add(StorageFilter.For(repository));            
        }
    }
}
