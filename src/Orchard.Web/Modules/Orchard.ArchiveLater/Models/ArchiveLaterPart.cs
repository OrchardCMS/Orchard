using System;
using Orchard.ContentManagement;
using Orchard.Utility;

namespace Orchard.ArchiveLater.Models {
    public class ArchiveLaterPart : ContentPart<ArchiveLaterPart> {
        private readonly LazyField<DateTime?> _scheduledArchiveUtc = new LazyField<DateTime?>();
        public LazyField<DateTime?> ScheduledArchiveUtc { get { return _scheduledArchiveUtc; } }
    }
}
