using System;
using System.Web.Mvc;
using System.Xml.Linq;
using JetBrains.Annotations;
using Orchard.Core.Feeds.Models;

namespace Orchard.Core.Feeds.Rss {
    [UsedImplicitly]
    public class RssFeedBuilder : IFeedBuilderProvider, IFeedBuilder {
        public FeedBuilderMatch Match(FeedContext context) {
            if (context.Format == "rss") {
                return new FeedBuilderMatch {
                    FeedBuilder = this,
                    Priority = -5
                };
            }
            return null;
        }

        public ActionResult Process(FeedContext context, Action populate) {
            var rss = new XElement("rss");
            rss.SetAttributeValue("version", "2.0");

            var channel = new XElement("channel");
            context.Response.Element = channel;
            rss.Add(channel);

            populate();

            return new RssResult(new XDocument(rss));
        }

        public FeedItem<TItem> AddItem<TItem>(FeedContext context, TItem item) {
            var feedItem = new FeedItem<TItem> {
                Item = item,
                Element = new XElement("item"),
            };
            context.Response.Items.Add(feedItem);
            context.Response.Element.Add(feedItem.Element);
            return feedItem;
        }

        public void AddProperty(FeedContext context, FeedItem feedItem, string name, string value) {
            if (feedItem == null) {
                context.Response.Element.Add(new XElement(name, value));
            }
            else {
                feedItem.Element.Add(new XElement(name, value));
            }
        }
    }

}
