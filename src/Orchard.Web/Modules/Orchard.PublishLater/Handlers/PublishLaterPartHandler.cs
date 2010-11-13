using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.PublishLater.Models;
using Orchard.PublishLater.Services;

namespace Orchard.PublishLater.Handlers {
    public class PublishLaterPartHandler : ContentHandler {
        private readonly IPublishLaterService _publishLaterService;

        public PublishLaterPartHandler(IPublishLaterService publishLaterService) {
            _publishLaterService = publishLaterService;

            OnLoaded<PublishLaterPart>((context, publishLater) => publishLater.ScheduledPublishUtc.Loader(delegate { return _publishLaterService.GetScheduledPublishUtc(publishLater.As<PublishLaterPart>()); }));
        }
    }
}