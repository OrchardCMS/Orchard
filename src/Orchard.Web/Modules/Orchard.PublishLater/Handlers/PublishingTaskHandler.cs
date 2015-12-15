using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;

namespace Orchard.PublishLater.Handlers {
    public class PublishingTaskHandler : IScheduledTaskHandler {
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;

        public PublishingTaskHandler(IContentManager contentManager, IOrchardServices orchardServices) {
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType == "Publish") {
                Logger.Information("Publishing item #{0} version {1} scheduled at {2} utc",
                    context.Task.ContentItem.Id,
                    context.Task.ContentItem.Version,
                    context.Task.ScheduledUtc);

                _contentManager.Publish(context.Task.ContentItem);
            }
        }
    }
}
