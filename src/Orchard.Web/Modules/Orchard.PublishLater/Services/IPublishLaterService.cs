using System;
using Orchard.ContentManagement;
using Orchard.PublishLater.Models;

namespace Orchard.PublishLater.Services {
    public interface IPublishLaterService : IDependency {
        DateTime? GetScheduledPublishUtc(PublishLaterPart publishLaterPart);
        void Publish(ContentItem contentItem, DateTime scheduledPublishUtc);
    }
}