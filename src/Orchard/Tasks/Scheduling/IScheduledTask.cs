using System;

namespace Orchard.ContentManagement.Aspects {
    public interface IScheduledTask  {
        string TaskType { get; }
        DateTime? ScheduledUtc { get; }
        ContentItem ContentItem { get; }
    }
}
