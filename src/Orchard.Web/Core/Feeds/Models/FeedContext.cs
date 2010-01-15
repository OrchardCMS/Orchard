using System.Collections.Generic;
using System.Web.Mvc;

namespace Orchard.Core.Feeds.Models {
    public class FeedContext {
        public FeedContext(IValueProvider valueProvider, string format) {
            ValueProvider = valueProvider;
            Format = format;
            Response = new FeedResponse();
            FeedData = new Dictionary<string, object>();
        }

        public IValueProvider ValueProvider { get; set; }
        public string Format { get; set; }

        public IFeedFormatter FeedFormatter { get; set; }

        public IDictionary<string, object> FeedData { get; set; }

        public FeedResponse Response { get; set; }
    }
}
