using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Parts;
using Orchard.Utility;

namespace Orchard.PublishLater.Models {
    public class PublishLaterPart : ContentPart<PublishLaterPart>, IPublishLaterPart {
        private readonly LazyField<DateTime?> _scheduledPublishUtc = new LazyField<DateTime?>();
        public LazyField<DateTime?> ScheduledPublishUtc { get { return _scheduledPublishUtc; } }
    }
}
