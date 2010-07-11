using System;
using Orchard.ContentManagement;

namespace Orchard.Core.Common.Services {
    public interface ICommonService : IDependency {
        DateTime? GetScheduledPublishUtc(ContentItem contentItem);
    }
}