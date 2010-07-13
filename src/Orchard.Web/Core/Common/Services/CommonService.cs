using System;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Tasks.Scheduling;

namespace Orchard.Core.Common.Services {
    public class CommonService : ICommonService {
        private readonly IPublishingTaskManager _publishingTaskManager;
        private readonly IContentManager _contentManager;

        public CommonService(IPublishingTaskManager publishingTaskManager, IContentManager contentManager) {
            _publishingTaskManager = publishingTaskManager;
            _contentManager = contentManager;
        }

        DateTime? ICommonService.GetScheduledPublishUtc(ContentItem contentItem) {
            var task = _publishingTaskManager.GetPublishTask(contentItem);
            return (task == null ? null : task.ScheduledUtc);
        }

        void ICommonService.Publish(ContentItem contentItem) {
            _publishingTaskManager.DeleteTasks(contentItem);
            _contentManager.Publish(contentItem);
        }

        void ICommonService.Publish(ContentItem contentItem, DateTime scheduledPublishUtc) {
            _publishingTaskManager.Publish(contentItem, scheduledPublishUtc);
        }

        DateTime? ICommonService.GetScheduledPublishUtc(CommonAspect commonAspect) {
            var task = _publishingTaskManager.GetPublishTask(commonAspect.ContentItem);
            return (task == null ? null : task.ScheduledUtc);
        }
    }
}