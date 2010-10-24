using System;
using Orchard.ArchiveLater.Models;
using Orchard.ContentManagement;

namespace Orchard.ArchiveLater.ViewModels {
    public class ArchiveLaterViewModel {
        private readonly ArchiveLaterPart _archiveLaterPart;

        public ArchiveLaterViewModel(ArchiveLaterPart archiveLaterPart) {
            _archiveLaterPart = archiveLaterPart;
        }

        public bool ArchiveLater { get; set; }
        public ContentItem ContentItem { get { return _archiveLaterPart.ContentItem; } }

        public bool IsPublished {
            get { return ContentItem.VersionRecord != null && ContentItem.VersionRecord.Published; }
        }

        public DateTime? ScheduledArchiveUtc { get; set; }

        public string ScheduledArchiveDate { get; set; }

        public string ScheduledArchiveTime { get; set; }
    }
}