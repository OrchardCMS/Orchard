using Orchard.ArchiveLater.Models;
using Orchard.ArchiveLater.Services;
using Orchard.ContentManagement.Handlers;

namespace Orchard.ArchiveLater.Handlers {
    public class ArchiveLaterPartHandler : ContentHandler {
        private readonly IArchiveLaterService _archiveLaterService;

        public ArchiveLaterPartHandler(IArchiveLaterService archiveLaterService) {
            _archiveLaterService = archiveLaterService;

            OnLoading<ArchiveLaterPart>((context, part) => LazyLoadHandlers(part));
            OnVersioning<ArchiveLaterPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));
            OnRemoved<ArchiveLaterPart>((context, part) => _archiveLaterService.RemoveArchiveLaterTasks(part.ContentItem));
            OnDestroyed<ArchiveLaterPart>((context, part) => _archiveLaterService.RemoveArchiveLaterTasks(part.ContentItem));
        }

        protected void LazyLoadHandlers(ArchiveLaterPart part) {
            part.ScheduledArchiveUtc.Loader(() => _archiveLaterService.GetScheduledArchiveUtc(part));
        }
    }
}
