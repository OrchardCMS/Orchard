using Orchard.ContentManagement.Handlers;
using Orchard.PublishLater.Models;
using Orchard.PublishLater.Services;
using Orchard.Tasks.Scheduling;

namespace Orchard.PublishLater.Handlers {
    public class PublishLaterPartHandler : ContentHandler {
        private readonly IPublishLaterService _publishLaterService;

        public PublishLaterPartHandler(
            IPublishLaterService publishLaterService,
            IPublishingTaskManager publishingTaskManager) {
            _publishLaterService = publishLaterService;

            OnLoading<PublishLaterPart>((context, part) => LazyLoadHandlers(part));
            OnVersioning<PublishLaterPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));
            OnRemoved<PublishLaterPart>((context, part) => publishingTaskManager.DeleteTasks(part.ContentItem));
            OnPublishing<PublishLaterPart>((context, part) =>
            {
                var existingPublishTask = publishingTaskManager.GetPublishTask(context.ContentItem);

                //Check if there is already and existing publish task for old version.
                if (existingPublishTask != null)
                {
                    //If exists remove it in order no to override the latest published version.
                    publishingTaskManager.DeleteTasks(context.ContentItem);
                }
            });
        }

        protected void LazyLoadHandlers(PublishLaterPart part) {
            part.ScheduledPublishUtc.Loader(() => _publishLaterService.GetScheduledPublishUtc(part));
        }
    }
}