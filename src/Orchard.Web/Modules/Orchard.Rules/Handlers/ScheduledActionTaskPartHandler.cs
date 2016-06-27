using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Rules.Models;

namespace Orchard.Rules.Handlers {
    public class ScheduledActionTaskPartHandler : ContentHandler {
        public ScheduledActionTaskPartHandler(IRepository<ScheduledActionTaskRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<ScheduledActionTaskPart>("ScheduledActionTask"));
        }
    }
}