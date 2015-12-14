using System;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Indexing.Models;
using Orchard.Logging;
using Orchard.Tasks.Indexing;
using Orchard.Services;

namespace Orchard.Indexing.Services {
    [UsedImplicitly]
    public class IndexingTaskManager : IIndexingTaskManager {
        private readonly IRepository<IndexingTaskRecord> _repository;
        private readonly IClock _clock;

        public IndexingTaskManager(
            IContentManager contentManager,
            IRepository<IndexingTaskRecord> repository,
            IClock clock) {
            _clock = clock;
            _repository = repository;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        private void CreateTask(ContentItem contentItem, int action) {
            if (contentItem == null) {
                throw new ArgumentNullException("contentItem");
            }

            if (contentItem.Record == null) {
                // ignore that case, when Update is called on a content item which has not be "created" yet

                return;
            }

            foreach (var task in _repository.Fetch(task => task.ContentItemRecord == contentItem.Record)) {
                _repository.Delete(task);
            }

            var taskRecord = new IndexingTaskRecord {
                CreatedUtc = _clock.UtcNow,
                ContentItemRecord = contentItem.Record,
                Action = action
            };

            _repository.Create(taskRecord);
            
        }

        public void CreateUpdateIndexTask(ContentItem contentItem) {

            CreateTask(contentItem, IndexingTaskRecord.Update);
            Logger.Information("Indexing task created for [{0}:{1}]", contentItem.ContentType, contentItem.Id);
        }

        public void CreateDeleteIndexTask(ContentItem contentItem) {

            CreateTask(contentItem, IndexingTaskRecord.Delete);
            Logger.Information("Deleting index task created for [{0}:{1}]", contentItem.ContentType, contentItem.Id);
        }
    }
}
