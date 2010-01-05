using Orchard.Core.Common.Records;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Common.Providers {
    public class BodyAspectHandler : ContentHandler {       
        public BodyAspectHandler(IRepository<BodyRecord> bodyRepository) {
            Filters.Add(StorageFilter.For(bodyRepository));
        }
    }
}
