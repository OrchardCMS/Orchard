using System;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Feeds.Models;

namespace Orchard.Core.Feeds.Services {
    public class CorePartsFeedItemBuilder : IFeedItemBuilder {
        private readonly IContentManager _contentManager;

        public CorePartsFeedItemBuilder(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public void Populate(FeedContext context) {
            foreach (var feedItem in context.Response.Items) {
                // locate parts
                var contentItem = feedItem.ContentItem;
                var metadata = _contentManager.GetItemMetadata(contentItem);
                var common = contentItem.Get<ICommonAspect>();
                var routable = contentItem.Get<RoutableAspect>();
                var body = contentItem.Get<BodyAspect>();

                // standard fields
                var link = "/todo";// metadata.DisplayRouteValues();

                var title = metadata.DisplayText;
                if (string.IsNullOrEmpty(title) && routable != null)
                    title = routable.Title;

                var contentText = title;
                if (body != null && !string.IsNullOrEmpty(body.Text))
                    contentText = body.Text;

                DateTime? publishedDate = null;
                if (common != null)
                    publishedDate = common.PublishedUtc ?? common.ModifiedUtc;

                // TODO: author


                // add to known formats
                if (context.Format == "rss") {
                    feedItem.Element.SetElementValue("title", title);
                    feedItem.Element.SetElementValue("link", link);
                    feedItem.Element.SetElementValue("description", contentText);
                    if (publishedDate != null)
                        feedItem.Element.SetElementValue("pubDate", publishedDate);//TODO: format
                    //feedItem.Data.SetElementValue("description", contentText);
                    feedItem.Element.Add(new XElement("guid", new XAttribute("isPermaLink", "true"), new XText(link)));
                }
                else {
                    context.FeedFormatter.AddProperty(context, feedItem, "link", link);
                    context.FeedFormatter.AddProperty(context, feedItem, "title", title);
                    context.FeedFormatter.AddProperty(context, feedItem, "description", contentText);
                    if (publishedDate != null)
                        context.FeedFormatter.AddProperty(context, feedItem, "published-date", Convert.ToString(publishedDate)); // format? cvt to generic T?
                }
            }
        }
    }
}