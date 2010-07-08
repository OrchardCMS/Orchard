using System;
using Orchard.ContentManagement;
using Orchard.Tasks.Scheduling;

namespace Orchard.Core.Common.Services {
    public class CommonService : ICommonService {
        private readonly IPublishingTaskManager _publishingTaskManager;

        public CommonService(IPublishingTaskManager publishingTaskManager) {
            _publishingTaskManager = publishingTaskManager;
        }

        public DateTime? GetScheduledPublishUtc(ContentItem contentItem) {
            var task = _publishingTaskManager.GetPublishTask(contentItem);
            return (task == null ? null : task.ScheduledUtc);
        }
    }
}