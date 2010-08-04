using Orchard.ArchiveLater.Models;
using Orchard.ArchiveLater.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ArchiveLater.Handlers {
    public class ArchiveLaterPartHandler : ContentHandler {
        private readonly IArchiveLaterService _archiveLaterService;

        public ArchiveLaterPartHandler(IArchiveLaterService archiveLaterService) {
            _archiveLaterService = archiveLaterService;

            OnLoaded<ArchiveLaterPart>((context, archiveLater) => archiveLater.ScheduledArchiveUtc.Loader(delegate { return _archiveLaterService.GetScheduledArchiveUtc(archiveLater.As<ArchiveLaterPart>()); }));
        }
    }
}