using System;

namespace Orchard.ContentManagement.Aspects {
    public interface IScheduledAspect : IContent {
        IScheduledTask Tasks { get; }
    }

    public interface IScheduledTask : IContent {
        string Action { get; set; }
        DateTime? ScheduledUtc { get; set; }
    }
}
