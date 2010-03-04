using JetBrains.Annotations;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;

namespace Orchard.Core.Scheduling.Services {
    [UsedImplicitly]
    public class PublishingTaskHandler : IScheduledTaskHandler {
        public PublishingTaskHandler(IOrchardServices services) {
            Services = services;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType == "Publish") {
                Logger.Information("Publishing item #{0} version {1} scheduled at {2} utc",
                    context.Task.ContentItem.Id,
                    context.Task.ContentItem.Version,
                    context.Task.ScheduledUtc);

                Services.ContentManager.Publish(context.Task.ContentItem);
            }
            else if (context.Task.TaskType == "Unpublish") {
                Logger.Information("Unpublishing item #{0} version {1} scheduled at {2} utc",
                    context.Task.ContentItem.Id,
                    context.Task.ContentItem.Version,
                    context.Task.ScheduledUtc);

                Services.ContentManager.Unpublish(context.Task.ContentItem);
            }
        }
    }
}
