using System;
using System.Linq;
using ArchiveLater.Models;
using Orchard.ContentManagement;
using Orchard.Tasks.Scheduling;

namespace ArchiveLater.Services {
    public class ArchiveLaterService : IArchiveLaterService {
        private const string UnpublishTaskType = "Unpublish";

        private readonly IScheduledTaskManager _scheduledTaskManager;

        public ArchiveLaterService(IScheduledTaskManager scheduledTaskManager) {
            _scheduledTaskManager = scheduledTaskManager;
        }

        void IArchiveLaterService.ArchiveLater(ContentItem contentItem, DateTime scheduledArchiveUtc) {
            RemoveArchiveLaterTasks(contentItem);
            _scheduledTaskManager.CreateTask(UnpublishTaskType, scheduledArchiveUtc, contentItem);
        }

        DateTime? IArchiveLaterService.GetScheduledArchiveUtc(ArchiveLaterPart archiveLaterPart) {
            var task = _scheduledTaskManager
                .GetTasks(archiveLaterPart.ContentItem)
                .Where(t => t.TaskType == UnpublishTaskType)
                .SingleOrDefault();
            
            return (task == null ? null : task.ScheduledUtc);
        }

        public void RemoveArchiveLaterTasks(ContentItem contentItem) {
            _scheduledTaskManager.DeleteTasks(contentItem, t => t.TaskType == UnpublishTaskType);
        }
    }
}