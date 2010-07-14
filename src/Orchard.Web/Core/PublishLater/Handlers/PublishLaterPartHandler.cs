using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.PublishLater.Models;
using Orchard.Core.PublishLater.Services;

namespace Orchard.Core.PublishLater.Handlers {
    public class PublishLaterPartHandler : ContentHandler {
        private readonly IPublishLaterService _publishLaterService;

        public PublishLaterPartHandler(IPublishLaterService publishLaterService) {
            _publishLaterService = publishLaterService;

            OnLoaded<PublishLaterPart>((context, publishLater) => publishLater.ScheduledPublishUtc.Loader(delegate { return _publishLaterService.GetScheduledPublishUtc(publishLater.As<PublishLaterPart>()); }));
        }
    }
}