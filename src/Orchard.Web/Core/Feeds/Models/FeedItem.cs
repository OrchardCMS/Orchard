using System.Xml.Linq;
using Orchard.ContentManagement;

namespace Orchard.Core.Feeds.Models {
    public class FeedItem {
        public ContentItem ContentItem { get; set; }
        public XElement Element { get; set; }
    }
}