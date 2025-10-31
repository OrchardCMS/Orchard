using Orchard.ArchiveLater.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.ViewModels;

namespace Orchard.ArchiveLater.ViewModels
{
    public class ArchiveLaterViewModel
    {
        private readonly ArchiveLaterPart _archiveLaterPart;

        public ArchiveLaterViewModel(ArchiveLaterPart archiveLaterPart)
        {
            _archiveLaterPart = archiveLaterPart;
        }

        public bool ArchiveLater { get; set; }
        public ContentItem ContentItem => _archiveLaterPart.ContentItem;

        public bool IsPublished => ContentItem.VersionRecord != null && ContentItem.VersionRecord.Published;

        public DateTimeEditor Editor { get; set; }
    }
}