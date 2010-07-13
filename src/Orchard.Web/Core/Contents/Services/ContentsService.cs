using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Tasks.Scheduling;

namespace Orchard.Core.Contents.Services {
    public class ContentsService : IContentsService {
        private readonly IContentManager _contentManager;
        private readonly IPublishingTaskManager _publishingTaskManager;

        public ContentsService(IContentManager contentManager, IPublishingTaskManager publishingTaskManager) {
            _contentManager = contentManager;
            _publishingTaskManager = publishingTaskManager;
        }

        void IContentsService.Publish(ContentItem contentItem) {
            _publishingTaskManager.DeleteTasks(contentItem);
            _contentManager.Publish(contentItem);
        }

        void IContentsService.Publish(ContentItem contentItem, DateTime scheduledPublishUtc) {
            _publishingTaskManager.Publish(contentItem, scheduledPublishUtc);
        }
    }
}