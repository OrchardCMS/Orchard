using Orchard.ContentManagement;
using Orchard.Tasks.Scheduling;

namespace Orchard.Core.Common.Services {
    public class CommonService : ICommonService {
        private readonly IPublishingTaskManager _publishingTaskManager;
        private readonly IContentManager _contentManager;

        public CommonService(IPublishingTaskManager publishingTaskManager, IContentManager contentManager) {
            _publishingTaskManager = publishingTaskManager;
            _contentManager = contentManager;
        }

        void ICommonService.Publish(ContentItem contentItem) {
            _publishingTaskManager.DeleteTasks(contentItem);
            _contentManager.Publish(contentItem);
        }
    }
}