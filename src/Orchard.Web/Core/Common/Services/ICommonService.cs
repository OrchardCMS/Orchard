using Orchard.ContentManagement;

namespace Orchard.Core.Common.Services {
    public interface ICommonService : IDependency {
        void Publish(ContentItem contentItem);
    }
}