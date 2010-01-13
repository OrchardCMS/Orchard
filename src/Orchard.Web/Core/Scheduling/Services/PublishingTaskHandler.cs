using Orchard.Logging;

namespace Orchard.Core.Scheduling.Services {
    public class PublishingTaskHandler : IScheduledTaskHandler {
        public PublishingTaskHandler(IOrchardServices services) {
            Services = services;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }

        public void Process(ScheduledTaskContext context) {
            if (context.ScheduledTaskRecord.Action == "Publish") {
                Logger.Information("Publishing item #{0} version {1} scheduled at {2} utc",
                    context.ContentItem.Id,
                    context.ContentItem.Version,
                    context.ScheduledTaskRecord.ScheduledUtc);

                Services.ContentManager.Publish(context.ContentItem);
            }
            else if (context.ScheduledTaskRecord.Action == "Unpublish") {
                Logger.Information("Unpublishing item #{0} version {1} scheduled at {2} utc",
                    context.ContentItem.Id,
                    context.ContentItem.Version,
                    context.ScheduledTaskRecord.ScheduledUtc);

                Services.ContentManager.Unpublish(context.ContentItem);
            }
        }
    }
}
