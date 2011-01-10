using Orchard.ContentManagement.Handlers;
using Orchard.PublishLater.Models;
using Orchard.PublishLater.Services;

namespace Orchard.PublishLater.Handlers {
    public class PublishLaterPartHandler : ContentHandler {
        private readonly IPublishLaterService _publishLaterService;

        public PublishLaterPartHandler(IPublishLaterService publishLaterService) {
            _publishLaterService = publishLaterService;

            OnLoading<PublishLaterPart>((context, part) => LazyLoadHandlers(part));
            OnVersioning<PublishLaterPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));
        }

        protected void LazyLoadHandlers(PublishLaterPart part) {
            part.ScheduledPublishUtc.Loader((value) => _publishLaterService.GetScheduledPublishUtc(part));
        }
    }
}