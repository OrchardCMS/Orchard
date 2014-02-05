using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;
using Orchard.Environment.Extensions;
using Orchard.Tags.Models;

namespace Orchard.Tags.Feeds {
    [OrchardFeature("Orchard.Tags.Feeds")]
    public class TagsFeedItemBuilder : IFeedItemBuilder {
        public void Populate(FeedContext context) {
            foreach (var feedItem in context.Response.Items.OfType<FeedItem<ContentItem>>()) {
                // add to known formats
                if (context.Format == "rss") {
                    
                    // adding tags to the rss item
                    var tagsPart = feedItem.Item.As<TagsPart>();
                    if (tagsPart != null) {
                        tagsPart.CurrentTags.ToList().ForEach(x => 
                            feedItem.Element.Add(new XElement("category", x)));
                    }
                }
            }
        }
    }
}