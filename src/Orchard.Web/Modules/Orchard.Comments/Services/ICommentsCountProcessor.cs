using Orchard.Events;

namespace Orchard.Comments.Services {
    public interface ICommentsCountProcessor : IEventHandler {
        void Process(int commentsPartId);
    }
}