using System;
using Orchard.ContentManagement;
using Orchard.Core.PublishLater.Models;
using Orchard.Tasks.Scheduling;

namespace Orchard.Core.PublishLater.Services {
    public class PublishLaterService : IPublishLaterService {
        private readonly IPublishingTaskManager _publishingTaskManager;

        public PublishLaterService(IPublishingTaskManager publishingTaskManager) {
            _publishingTaskManager = publishingTaskManager;
        }

        void IPublishLaterService.Publish(ContentItem contentItem, DateTime scheduledPublishUtc) {
            _publishingTaskManager.Publish(contentItem, scheduledPublishUtc);
        }

        DateTime? IPublishLaterService.GetScheduledPublishUtc(PublishLaterPart publishLaterPart) {
            IScheduledTask task = _publishingTaskManager.GetPublishTask(publishLaterPart.ContentItem);
            return (task == null ? null : task.ScheduledUtc);
        }
    }
}