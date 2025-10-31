using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Orchard.ArchiveLater.Models
{
    public class ArchiveLaterPart : ContentPart
    {
        public LazyField<DateTime?> ScheduledArchiveUtc { get; } = new LazyField<DateTime?>();
    }
}
