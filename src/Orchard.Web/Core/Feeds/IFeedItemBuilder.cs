using Orchard.Core.Feeds.Models;
using Orchard.Events;

namespace Orchard.Core.Feeds {
    public interface IFeedItemBuilder : IEventHandler {
        void Populate(FeedContext context);
    }
}
