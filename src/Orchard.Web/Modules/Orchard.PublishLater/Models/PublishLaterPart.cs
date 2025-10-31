using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Utilities;

namespace Orchard.PublishLater.Models
{
    public class PublishLaterPart : ContentPart<PublishLaterPart>, IPublishingControlAspect
    {
        public LazyField<DateTime?> ScheduledPublishUtc { get; } = new LazyField<DateTime?>();
    }
}
