using System;
using Orchard.ArchiveLater.Models;
using Orchard.ContentManagement;

namespace Orchard.ArchiveLater.Services {
    public interface IArchiveLaterService : IDependency {
        DateTime? GetScheduledArchiveUtc(ArchiveLaterPart archiveLaterPart);
        void ArchiveLater(ContentItem contentItem, DateTime scheduledArchiveUtc);
        void RemoveArchiveLaterTasks(ContentItem contentItem);
    }
}