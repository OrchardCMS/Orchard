using Orchard.ContentManagement.Aspects;

namespace Orchard.Tasks.Scheduling {
    public class ScheduledTaskContext {
        public IScheduledTask Task { get; set; }
    }
}