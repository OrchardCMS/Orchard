using System.Linq;
using Orchard.Tasks;
using Orchard.Tasks.Indexing;

namespace Orchard.Indexing.Services {
    public class CreateUpdateIndexTaskBackgroundTask : IBackgroundTask {
        private readonly IIndexTaskBatchManagementService _indexTaskBatchManagementService;
        private readonly IIndexingTaskManager _indexingTaskManager;

        public CreateUpdateIndexTaskBackgroundTask(IIndexTaskBatchManagementService indexTaskBatchManagementService, IIndexingTaskManager indexingTaskManager) {
            _indexTaskBatchManagementService = indexTaskBatchManagementService;
            _indexingTaskManager = indexingTaskManager;
        }

        public void Sweep() {
            var contentItemsLists = _indexTaskBatchManagementService.GetNextBatchOfContentItemsToIndex();

            foreach (var contentItemsList in contentItemsLists) {
                foreach (var contentItem in contentItemsList) {
                    _indexingTaskManager.CreateUpdateIndexTask(contentItem);
                }
            }
        }
    }
}