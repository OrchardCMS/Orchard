using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Scheduling.Models;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Tasks;

namespace Orchard.Core.Scheduling.Services {
    [UsedImplicitly]
    public class SchedulingBackgroundTask : IBackgroundTask {
        private readonly IClock _clock;
        private readonly IRepository<ScheduledTaskRecord> _repository;
        private readonly IEnumerable<IScheduledTaskHandler> _handlers;

        public SchedulingBackgroundTask(
            IOrchardServices services,
            IClock clock,
            IRepository<ScheduledTaskRecord> repository,
            IEnumerable<IScheduledTaskHandler> handlers) {
            _clock = clock;
            _repository = repository;
            _handlers = handlers;
            Services = services;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }

        public void Sweep() {
            var taskEntries = _repository.Fetch(x => x.ScheduledUtc <= _clock.UtcNow)
                .Select(x => new { x.Id, x.Action })
                .ToArray();

            foreach (var taskEntry in taskEntries) {
                //TODO: start a dedicated transaction scope

                try {
                    // fetch the task
                    var context = new ScheduledTaskContext {
                        ScheduledTaskRecord = _repository.Get(taskEntry.Id)
                    };

                    // another node in the farm has performed this work before us
                    if (context.ScheduledTaskRecord == null) {
                        continue;
                    }

                    // removing record first helps avoid concurrent execution
                    _repository.Delete(context.ScheduledTaskRecord);

                    // if it's associaged with a version of a content item
                    if (context.ScheduledTaskRecord.ScheduledAspectRecord != null) {
                        var versionRecord = context.ScheduledTaskRecord.ScheduledAspectRecord.ContentItemVersionRecord;

                        // hydrate that item as part of the task context
                        context.ContentItem = Services.ContentManager.Get(
                            versionRecord.ContentItemRecord.Id,
                            VersionOptions.VersionRecord(versionRecord.Id));
                    }

                    // dispatch to standard or custom handlers
                    foreach(var handler in _handlers) {
                        handler.Process(context);
                    }

                    //TODO: commit dedicated scope
                }
                catch (Exception ex) {
                    Logger.Warning(ex, "Unable to process scheduled task #{0} of type {1}", taskEntry.Id, taskEntry.Action);

                    //TODO: handle exception to rollback dedicated xact, and re-delete task record. 
                    // does this also need some retry logic?
                }
            }
        }
    }
}
