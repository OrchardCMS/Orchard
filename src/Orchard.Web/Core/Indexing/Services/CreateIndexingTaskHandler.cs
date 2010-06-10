using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Tasks.Indexing;

namespace Orchard.Core.Indexing.Services {
    /// <summary>
    /// Intercepts the ContentHandler events to create indexing tasks when a content item 
    /// is published, and to delete them when the content item is unpublished.
    /// </summary>
    public class CreateIndexingTaskHandler : ContentHandler {
        private readonly IIndexingTaskManager _indexingTaskManager;

        public CreateIndexingTaskHandler(IIndexingTaskManager indexingTaskManager) {
            _indexingTaskManager = indexingTaskManager;

            OnPublishing<ContentPart<CommonRecord>>(CreateIndexingTask);
            OnRemoved<ContentPart<CommonRecord>>(RemoveIndexingTask);
        }

        void CreateIndexingTask(PublishContentContext context, ContentPart<CommonRecord> part) {
            _indexingTaskManager.CreateUpdateIndexTask(context.ContentItem);
        }

        void RemoveIndexingTask(RemoveContentContext context, ContentPart<CommonRecord> part) {
            _indexingTaskManager.CreateDeleteIndexTask(context.ContentItem);
        }

    }
}
