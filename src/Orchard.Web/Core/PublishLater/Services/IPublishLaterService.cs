using System;
using Orchard.ContentManagement;
using Orchard.Core.PublishLater.Models;

namespace Orchard.Core.PublishLater.Services {
    public interface IPublishLaterService : IDependency {
        DateTime? GetScheduledPublishUtc(PublishLaterPart publishLaterPart);
        void Publish(ContentItem contentItem, DateTime scheduledPublishUtc);
    }
}