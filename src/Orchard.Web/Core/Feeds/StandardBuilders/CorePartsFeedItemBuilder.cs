using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Feeds.Models;

namespace Orchard.Core.Feeds.StandardBuilders {
    [UsedImplicitly]
    public class CorePartsFeedItemBuilder : IFeedItemBuilder, IDependency {
        private readonly IContentManager _contentManager;
        private readonly RouteCollection _routes;

        public CorePartsFeedItemBuilder(IContentManager contentManager, RouteCollection routes) {
            _contentManager = contentManager;
            _routes = routes;
        }

        public void Populate(FeedContext context) {
            foreach (var feedItem in context.Response.Items.OfType<FeedItem<ContentItem>>()) {

                var inspector = new ItemInspector(
                    feedItem.Item,
                    _contentManager.GetItemMetadata(feedItem.Item));


                // TODO: author


                // add to known formats
                if (context.Format == "rss") {
                    var link = new XElement("link");
                    var guid = new XElement("guid", new XAttribute("isPermaLink", "true"));

                    context.Response.Contextualize(requestContext => {
                                                       var urlHelper = new UrlHelper(requestContext, _routes);
                                                       link.Add(urlHelper.RouteUrl(inspector.Link));
                                                       guid.Add(urlHelper.RouteUrl(inspector.Link));
                                                   });

                    feedItem.Element.SetElementValue("title", inspector.Title);
                    feedItem.Element.Add(link);
                    feedItem.Element.SetElementValue("description", inspector.Description);
                    if (inspector.PublishedUtc != null)
                        feedItem.Element.SetElementValue("pubDate", inspector.PublishedUtc);//TODO: format
                    feedItem.Element.Add(guid);
                }
                else {
                    var feedItem1 = feedItem;
                    context.Response.Contextualize(requestContext => {
                                                       var urlHelper = new UrlHelper(requestContext, _routes);
                                                       context.Builder.AddProperty(context, feedItem1, "link", urlHelper.RouteUrl(inspector.Link));
                                                   });
                    context.Builder.AddProperty(context, feedItem, "title", inspector.Title);
                    context.Builder.AddProperty(context, feedItem, "description", inspector.Description);

                    if (inspector.PublishedUtc != null)
                        context.Builder.AddProperty(context, feedItem, "published-date", Convert.ToString(inspector.PublishedUtc)); // format? cvt to generic T?
                }
            }
        }
    }
}