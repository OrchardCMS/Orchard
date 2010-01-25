using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Scheduling.Models;
using Orchard.Core.Scheduling.Records;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.Utility;

namespace Orchard.Core.Scheduling.Services {
    public class ScheduledTaskManager : IScheduledTaskManager {
        private readonly IRepository<ScheduledTaskRecord> _repository;

        public ScheduledTaskManager(
            IOrchardServices services,
            IRepository<ScheduledTaskRecord> repository) {
            _repository = repository;
            Services = services;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
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
                .Select(x => new Task(Services.ContentManager, x))
                .Cast<IScheduledTask>()
                .ToReadOnlyCollection();
        }

        public void DeleteTasks(ContentItem contentItem, Func<IScheduledTask, bool> predicate) {
            //TEMP: Is this thread safe? Does it matter?
            var tasks = _repository
                .Fetch(x => x.ContentItemVersionRecord.ContentItemRecord == contentItem.Record);

            foreach (var task in tasks) {
                if (predicate(new Task(Services.ContentManager, task))) {
                    _repository.Delete(task);
                }
            }
        }
    }
}
