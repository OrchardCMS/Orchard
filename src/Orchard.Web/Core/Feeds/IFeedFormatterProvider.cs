using Orchard.Core.Feeds.Models;

namespace Orchard.Core.Feeds {
    public interface IFeedFormatterProvider : IDependency {
        FeedFormatterMatch Match(FeedContext context);
    }
    
    public class FeedFormatterMatch {
        public int Priority { get; set; }
        public IFeedFormatter FeedFormatter { get; set; }
    }
}
