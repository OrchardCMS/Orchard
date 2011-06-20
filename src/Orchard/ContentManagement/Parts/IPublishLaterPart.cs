using System;
using Orchard.ContentManagement.Utilities;

namespace Orchard.ContentManagement.Parts {
    public interface IPublishLaterPart {
        LazyField<DateTime?> ScheduledPublishUtc { get; }
    }
}