using System.Web.Mvc;

namespace Orchard.Core.Feeds.Models {
    public class FeedContext {
        public FeedContext(IValueProvider valueProvider, string format) {
            ValueProvider = valueProvider;
            Format = format;
            Response = new FeedResponse();
        }

        public IValueProvider ValueProvider { get; set; }
        public string Format { get; set; }
        public FeedResponse Response { get; set; }

        public IFeedBuilder Builder { get; set; }
    }
}
