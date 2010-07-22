using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Tasks.Indexing;

namespace Orchard.Indexing.Handlers {
    /// <summary>
    /// Intercepts the ContentHandler events to create indexing tasks when a content item 
    /// is published, and to delete them when the content item is unpublished.
    /// </summary>
    public class CreateIndexingTaskHandler : ContentHandler {
        private readonly IIndexingTaskManager _indexingTaskManager;

        public CreateIndexingTaskHandler(IIndexingTaskManager indexingTaskManager) {
            _indexingTaskManager = indexingTaskManager;

            OnPublishing<ContentPart<CommonPartRecord>>(CreateIndexingTask);
            OnRemoved<ContentPart<CommonPartRecord>>(RemoveIndexingTask);
        }

        void CreateIndexingTask(PublishContentContext context, ContentPart<CommonPartRecord> part) {
            _indexingTaskManager.CreateUpdateIndexTask(context.ContentItem);
        }

        void RemoveIndexingTask(RemoveContentContext context, ContentPart<CommonPartRecord> part) {
            _indexingTaskManager.CreateDeleteIndexTask(context.ContentItem);
        }

    }
}
