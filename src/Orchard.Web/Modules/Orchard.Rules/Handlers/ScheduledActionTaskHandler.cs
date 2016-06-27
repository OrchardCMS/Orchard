using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Rules.Models;
using Orchard.Rules.Services;
using Orchard.Tasks.Scheduling;

namespace Orchard.Rules.Handlers {
    public class ScheduledActionTaskHandler : IScheduledTaskHandler {
        private readonly IRulesManager _rulesManager;

        public ScheduledActionTaskHandler(
            IRulesManager rulesManager) {
            _rulesManager = rulesManager;
        }

        public ILogger Logger { get; set; }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType == "TriggerRule") {
                Logger.Information("Triggering Rule item #{0} version {1} scheduled at {2} utc",
                                   context.Task.ContentItem.Id,
                                   context.Task.ContentItem.Version,
                                   context.Task.ScheduledUtc);

                var scheduledActionTask = context.Task.ContentItem.As<ScheduledActionTaskPart>();

                _rulesManager.ExecuteActions(scheduledActionTask.ScheduledActions.Select(x => x.ActionRecord), new Dictionary<string, object>());
            }
        }
    }
}
