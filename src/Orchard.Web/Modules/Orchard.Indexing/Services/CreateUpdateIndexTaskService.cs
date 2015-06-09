using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Tasks.Indexing;

namespace Orchard.Indexing.Services {
    public interface IJobsQueueService : IEventHandler {
        void Enqueue(string message, object parameters, int priority);
    }

    public class CreateUpdateIndexTaskService : ICreateUpdateIndexTaskService {
        private readonly IContentManager _contentManager;
        private readonly IJobsQueueService _jobsQueueService;
        private readonly IIndexingTaskManager _indexingTaskManager;

        private const int BatchSize = 50;
        public const int JobPriority = 10;

        public CreateUpdateIndexTaskService(IContentManager contentManager, IIndexingTaskManager indexingTaskManager, IJobsQueueService jobsQueueService) {
            _jobsQueueService = jobsQueueService;
            _indexingTaskManager = indexingTaskManager;
            _contentManager = contentManager;

        }

        public void CreateNextIndexingTaskBatch(string contentTypeName, string currentBatchIndex) {
            var contentItems = _contentManager.Query(contentTypeName).Slice(int.Parse(currentBatchIndex), BatchSize).ToList();

            foreach (var contentItem in contentItems) {
                _indexingTaskManager.CreateUpdateIndexTask(contentItem);
            }

            if (contentItems.Count == BatchSize) {
                _jobsQueueService.Enqueue("ICreateUpdateIndexTaskService.CreateNextIndexingTaskBatch", new Dictionary<string, object> { { "contentTypeName", contentTypeName }, { "currentBatchIndex", (int.Parse(currentBatchIndex) + BatchSize).ToString() } }, JobPriority);
            }
        }
    }
}