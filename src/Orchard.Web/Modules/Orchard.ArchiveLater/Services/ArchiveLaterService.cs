using System;
using System.Linq;
using Orchard.ArchiveLater.Models;
using Orchard.ContentManagement;
using Orchard.Core.Contents;
using Orchard.Localization;
using Orchard.Tasks.Scheduling;

namespace Orchard.ArchiveLater.Services {
    public class ArchiveLaterService : IArchiveLaterService {
        private const string UnpublishTaskType = "Unpublish";

        private readonly IScheduledTaskManager _scheduledTaskManager;

        public ArchiveLaterService(
            IOrchardServices services,
            IScheduledTaskManager scheduledTaskManager) {
            Services = services;
            _scheduledTaskManager = scheduledTaskManager;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        void IArchiveLaterService.ArchiveLater(ContentItem contentItem, DateTime scheduledArchiveUtc) {
            if (!Services.Authorizer.Authorize(Permissions.PublishContent, contentItem, T("Couldn't archive selected content.")))
                return;

            RemoveArchiveLaterTasks(contentItem);
            _scheduledTaskManager.CreateTask(UnpublishTaskType, scheduledArchiveUtc, contentItem);
        }

        DateTime? IArchiveLaterService.GetScheduledArchiveUtc(ArchiveLaterPart archiveLaterPart) {
            var task = _scheduledTaskManager.GetTasks(archiveLaterPart.ContentItem)
                .SingleOrDefault(t => t.TaskType == UnpublishTaskType);
            
            return (task == null ? null : task.ScheduledUtc);
        }

        public void RemoveArchiveLaterTasks(ContentItem contentItem) {
            _scheduledTaskManager.DeleteTasks(contentItem, t => t.TaskType == UnpublishTaskType);
        }
    }
}