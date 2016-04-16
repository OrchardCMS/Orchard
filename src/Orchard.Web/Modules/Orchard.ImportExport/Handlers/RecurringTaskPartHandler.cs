using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Handlers {
    [OrchardFeature("Orchard.Deployment")]
    public class RecurringTaskPartHandler : ContentHandler {
        public RecurringTaskPartHandler(IRepository<RecurringTaskPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
