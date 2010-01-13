using Orchard.ContentManagement;
using Orchard.Core.Scheduling.Models;

namespace Orchard.Core.Scheduling {
    public class ScheduledTaskContext {
        public ScheduledTaskRecord ScheduledTaskRecord { get; set; }
        public ContentItem ContentItem { get; set; }
    }
}