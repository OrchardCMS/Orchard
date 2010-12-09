using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Common.Scheduling;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;

namespace Orchard.PublishLater.Handlers {
    [UsedImplicitly]
    public class PublishingTaskHandler : OwnedScheduledTaskHandler {
        private readonly IContentManager _contentManager;

        public PublishingTaskHandler(IContentManager contentManager, IOrchardServices orchardServices) : base(orchardServices) {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public override void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType == "Publish") {
                Logger.Information("Publishing item #{0} version {1} scheduled at {2} utc",
                    context.Task.ContentItem.Id,
                    context.Task.ContentItem.Version,
                    context.Task.ScheduledUtc);

                SetCurrentUser(context.Task.ContentItem);
                _contentManager.Publish(context.Task.ContentItem);
            }
        }
    }
}
