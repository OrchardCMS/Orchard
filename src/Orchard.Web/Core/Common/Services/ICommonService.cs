using System;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;

namespace Orchard.Core.Common.Services {
    public interface ICommonService : IDependency {
        DateTime? GetScheduledPublishUtc(ContentItem contentItem);
        void Publish(ContentItem contentItem);
        void Publish(ContentItem contentItem, DateTime scheduledPublishUtc);
        DateTime? GetScheduledPublishUtc(CommonAspect commonAspect);
    }
}