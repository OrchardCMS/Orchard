using System;
using Orchard.Utility;

namespace Orchard.ContentManagement.Parts {
    public interface IPublishLaterPart {
        LazyField<DateTime?> ScheduledPublishUtc { get; }
    }
}