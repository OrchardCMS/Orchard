using System;
using System.Collections.Generic;
using System.Web.Routing;
using System.Xml.Linq;

namespace Orchard.Core.Feeds.Models {
    public class FeedResponse {
        public FeedResponse() {
            Items = new List<FeedItem>();
            Contextualizers = new List<Action<RequestContext>>();
        }

        public IList<FeedItem> Items { get; set; }
        public XElement Element { get; set; }
        public IList<Action<RequestContext>> Contextualizers { get; set; }

        public void Contextualize(Action<RequestContext> contextualizer) {
            Contextualizers.Add(contextualizer);
        }
    }
}
