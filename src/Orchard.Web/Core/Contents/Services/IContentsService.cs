using System;
using Orchard.ContentManagement;

namespace Orchard.Core.Contents.Services {
    public interface IContentsService : IDependency {
        void Publish(ContentItem contentItem);
        void Publish(ContentItem contentItem, DateTime scheduledPublishUtc);
    }
}
