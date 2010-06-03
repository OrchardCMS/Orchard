using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.Utility.Extensions;
using Orchard.Tasks.Indexing;
using Orchard.Core.Indexing.Models;
using Orchard.Services;

namespace Orchard.Core.Indexing.Services {
    [UsedImplicitly]
    public class IndexingTaskManager : IIndexingTaskManager {
        private readonly IContentManager _contentManager;
        private readonly IRepository<IndexingTaskRecord> _repository;
        private readonly IRepository<IndexingSettingsRecord> _settings;
        private readonly IClock _clock;

        public IndexingTaskManager(
            IContentManager contentManager,
            IRepository<IndexingTaskRecord> repository,
            IRepository<IndexingSettingsRecord> settings,
            IClock clock) {
            _clock = clock;
            _repository = repository;
            _contentManager = contentManager;
            _settings = settings;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        private void CreateTask(ContentItem contentItem, int action) {
            if ( contentItem == null ) {
                throw new ArgumentNullException("contentItem");
            }

            // remove previous tasks for the same content item
            var tasks = _repository
                .Fetch(x => x.ContentItemRecord.Id == contentItem.Id)
                .ToArray();

            foreach ( var task in tasks ) {
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

        public IEnumerable<IIndexingTask> GetTasks(DateTime? createdAfter) {
            return _repository
                .Fetch(x => x.CreatedUtc > createdAfter)
                .Select(x => new IndexingTask(_contentManager, x))
                .Cast<IIndexingTask>()
                .ToReadOnlyCollection();
        }

        public void DeleteTasks(DateTime? createdBefore) {
            Logger.Debug("Deleting Indexing tasks created before {0}", createdBefore);

            var tasks = _repository
                .Fetch(x => x.CreatedUtc <= createdBefore);

            foreach (var task in tasks) {
                _repository.Delete(task);
            }
        }

        public void DeleteTasks(ContentItem contentItem) {
            Logger.Debug("Deleting Indexing tasks for ContentItem [{0}:{1}]", contentItem.ContentType, contentItem.Id);

            var tasks = _repository
                .Fetch(x => x.Id == contentItem.Id);

            foreach (var task in tasks) {
                _repository.Delete(task);
            }
        }

        public void RebuildIndex() {
            var settingsRecord = _settings.Table.FirstOrDefault();
            if (settingsRecord == null) {
                _settings.Create(settingsRecord = new IndexingSettingsRecord() );
            }
            
            settingsRecord.LatestIndexingUtc = new DateTime(1980, 1, 1);
            _settings.Update(settingsRecord);
        }

    }
}
