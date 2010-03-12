using JetBrains.Annotations;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Common.Handlers {
    [UsedImplicitly]
    public class BodyAspectHandler : ContentHandler {       
        public BodyAspectHandler(IRepository<BodyRecord> bodyRepository) {
            Filters.Add(StorageFilter.For(bodyRepository));
        }
    }
}