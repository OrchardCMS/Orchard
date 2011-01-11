using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement;
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

            OnPublished<ContentPart>(CreateIndexingTask);
            OnUnpublished<ContentPart>(CreateIndexingTask);
            OnRemoved<ContentPart>(RemoveIndexingTask);
        }

        void CreateIndexingTask(PublishContentContext context, ContentPart part) {
            // "Unpublish" case: Same as "remove"
            if (context.PublishingItemVersionRecord == null) {
                _indexingTaskManager.CreateDeleteIndexTask(context.ContentItem);
                return;
            }
            // "Publish" case: update index
            _indexingTaskManager.CreateUpdateIndexTask(context.ContentItem);
        }

        void RemoveIndexingTask(RemoveContentContext context, ContentPart part) {
            _indexingTaskManager.CreateDeleteIndexTask(context.ContentItem);
        }
    }
}
