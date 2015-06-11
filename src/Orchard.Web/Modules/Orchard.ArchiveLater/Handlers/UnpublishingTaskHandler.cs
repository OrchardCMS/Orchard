using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;

namespace Orchard.ArchiveLater.Handlers {
    [UsedImplicitly]
    public class UnpublishingTaskHandler : IScheduledTaskHandler {
        private readonly IContentManager _contentManager;

        public UnpublishingTaskHandler(IContentManager contentManager, IOrchardServices orchardServices) {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType == "Unpublish") {
                Logger.Information("Unpublishing item #{0} version {1} scheduled at {2} utc",
                    context.Task.ContentItem.Id,
                    context.Task.ContentItem.Version,
                    context.Task.ScheduledUtc);

                _contentManager.Unpublish(context.Task.ContentItem);
            }
        }
    }
}
