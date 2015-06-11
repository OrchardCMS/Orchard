using System;
using Orchard.ContentManagement.Utilities;

namespace Orchard.ContentManagement.Aspects {
    public interface IPublishingControlAspect {
        LazyField<DateTime?> ScheduledPublishUtc { get; }
    }
}