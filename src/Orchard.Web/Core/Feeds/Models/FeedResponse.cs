using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Orchard.Core.Feeds.Models {
    public class FeedResponse {
        public FeedResponse() {
            Items = new List<FeedItem>();
        }

        public IList<FeedItem> Items { get; set; }
        public XElement Element { get; set; }
        public ActionResult Result { get; set; }
    }
}