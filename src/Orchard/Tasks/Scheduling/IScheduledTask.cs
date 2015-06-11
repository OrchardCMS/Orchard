using System;
using Orchard.ContentManagement;

namespace Orchard.Tasks.Scheduling {
    public interface IScheduledTask  {
        string TaskType { get; }
        DateTime? ScheduledUtc { get; }
        ContentItem ContentItem { get; }
    }
}