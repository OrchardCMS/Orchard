using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.Core.Feeds.Models;
using Orchard.Mvc.Extensions;
using Orchard.Services;

namespace Orchard.Core.Feeds.StandardBuilders {
    public class CorePartsFeedItemBuilder : IFeedItemBuilder {
        private readonly IContentManager _contentManager;
        private readonly RouteCollection _routes;
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;

        public CorePartsFeedItemBuilder(
            IContentManager contentManager, 
            RouteCollection routes,
            IEnumerable<IHtmlFilter> htmlFilters) {
            _contentManager = contentManager;
            _routes = routes;
            _htmlFilters = htmlFilters;
        }

        public void Populate(FeedContext context) {
            foreach (var feedItem in context.Response.Items.OfType<FeedItem<ContentItem>>()) {

                var inspector = new ItemInspector(
                    feedItem.Item,
                    _contentManager.GetItemMetadata(feedItem.Item), 
                    _htmlFilters);

                // author is intentionally left empty as it could result in unwanted spam

                // add to known formats
                if (context.Format == "rss") {
                    var link = new XElement("link");
                    var guid = new XElement("guid", new XAttribute("isPermaLink", "true"));

                    context.Response.Contextualize(requestContext => {
                                                        var urlHelper = new UrlHelper(requestContext, _routes);
                                                        var uriBuilder = new UriBuilder(urlHelper.MakeAbsolute("/")) { Path = urlHelper.RouteUrl(inspector.Link) };
                                                        link.Add(uriBuilder.Uri.OriginalString);
                                                        guid.Add(uriBuilder.Uri.OriginalString);
                                                   });

                    feedItem.Element.SetElementValue("title", inspector.Title);
                    feedItem.Element.Add(link);
                    feedItem.Element.SetElementValue("description", inspector.Description);

                    if ( inspector.PublishedUtc != null ) {
                        // RFC833 
                        // The "R" or "r" standard format specifier represents a custom date and time format string that is defined by 
                        // the DateTimeFormatInfo.RFC1123Pattern property. The pattern reflects a defined standard, and the property  
                        // is read-only. Therefore, it is always the same, regardless of the culture used or the format provider supplied.  
                        // The custom format string is "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'". When this standard format specifier is used,  
                        // the formatting or parsing operation always uses the invariant culture. 
                        feedItem.Element.SetElementValue("pubDate", inspector.PublishedUtc.Value.ToString("r"));
                    }

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