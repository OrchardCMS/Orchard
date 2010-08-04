using System;
using ArchiveLater.Models;
using Orchard;
using Orchard.ContentManagement;

namespace ArchiveLater.Services {
    public interface IArchiveLaterService : IDependency {
        DateTime? GetScheduledArchiveUtc(ArchiveLaterPart archiveLaterPart);
        void ArchiveLater(ContentItem contentItem, DateTime scheduledArchiveUtc);
        void RemoveArchiveLaterTasks(ContentItem contentItem);
    }
}