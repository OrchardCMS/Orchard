using Orchard.Events;

namespace Orchard.Blogs.Services {
    public interface IBlogPostsCountProcessor : IEventHandler {
        void Process(int blogPartId);
    }
}