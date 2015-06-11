using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Scheduling.Models;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.Utility.Extensions;

namespace Orchard.Core.Scheduling.Services {
    [UsedImplicitly]
    public class ScheduledTaskManager : IScheduledTaskManager {
        private readonly IContentManager _contentManager;
        private readonly IRepository<ScheduledTaskRecord> _repository;

        public ScheduledTaskManager(
            IContentManager contentManager,
            IRepository<ScheduledTaskRecord> repository) {
            _repository = repository;
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void CreateTask(string action, DateTime scheduledUtc, ContentItem contentItem) {
            var taskRecord = new ScheduledTaskRecord {
                TaskType = action,
                ScheduledUtc = scheduledUtc,
            };
            if (contentItem != null) {
                taskRecord.ContentItemVersionRecord = contentItem.VersionRecord;
            }
            _repository.Create(taskRecord);
        }

        public IEnumerable<IScheduledTask> GetTasks(ContentItem contentItem) {
            return _repository
                .Fetch(x => x.ContentItemVersionRecord.ContentItemRecord == contentItem.Record)
                .Select(x => new Task(_contentManager, x))
                .Cast<IScheduledTask>()
                .ToReadOnlyCollection();
        }

        public IEnumerable<IScheduledTask> GetTasks(string taskType, DateTime? scheduledBeforeUtc = null) {
            var query = scheduledBeforeUtc != null 
                ? _repository.Fetch(t => t.TaskType == taskType && t.ScheduledUtc <= scheduledBeforeUtc)
                : _repository.Fetch(t => t.TaskType == taskType);

            return 
                query.Select(x => new Task(_contentManager, x))
                .Cast<IScheduledTask>()
                .ToReadOnlyCollection();
        }

        public void DeleteTasks(ContentItem contentItem, Func<IScheduledTask, bool> predicate = null ) {
            // if contentItem is null, all tasks are used
            var tasks = contentItem == null ? _repository.Table : _repository
                .Fetch(x => x.ContentItemVersionRecord.ContentItemRecord == contentItem.Record);

            foreach (var task in tasks) {
                if (predicate == null || predicate(new Task(_contentManager, task))) {
                    _repository.Delete(task);
                }
            }
            
            _repository.Flush();
        }
    }
}
