using Orchard.Core.Feeds.Models;

namespace Orchard.Core.Feeds {
    public interface IFeedQuery {
        void Execute(FeedContext context);
    }
}