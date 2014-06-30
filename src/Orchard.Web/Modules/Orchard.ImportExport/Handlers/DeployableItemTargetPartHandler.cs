using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Handlers {
    [OrchardFeature("Orchard.Deployment")]
    public class DeployableItemTargetPartHandler : ContentHandler {
        public DeployableItemTargetPartHandler(IRepository<DeployableItemTargetPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}