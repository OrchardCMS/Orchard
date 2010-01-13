using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.Core.Scheduling.Models;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Utility;

namespace Orchard.Core.Scheduling.Services {
    public interface IScheduledTaskManager : IDependency {
        void CreateTask(string action, DateTime scheduledUtc, ContentItem contentItem);
        IEnumerable<ScheduledTaskRecord> GetTasks(ContentItem contentItem);
    }

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
                Action = action,
                ScheduledUtc = scheduledUtc,
            };
            if (contentItem != null) {
                var part = contentItem.Get<ContentPart<ScheduledAspectRecord>>();
                if (part != null) {
                    taskRecord.ScheduledAspectRecord = part.Record;
                }
            }
            _repository.Create(taskRecord);
        }

        public IEnumerable<ScheduledTaskRecord> GetTasks(ContentItem contentItem) {
            return _repository
                .Fetch(x => x.ScheduledAspectRecord.ContentItemRecord == contentItem.Record)
                .ToReadOnlyCollection();
        }
    }
}
