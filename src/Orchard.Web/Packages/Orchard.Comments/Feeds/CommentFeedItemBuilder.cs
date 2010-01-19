using System;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.Comments.Models;
using Orchard.ContentManagement;
using Orchard.Core.Feeds;
using Orchard.Core.Feeds.Models;
using Orchard.Core.Feeds.Services;
using Orchard.Localization;

namespace Orchard.Comments.Feeds {
    public class CommentFeedItemBuilder : IFeedItemBuilder {
        private readonly IContentManager _contentManager;

        public CommentFeedItemBuilder(
            IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }

        public void Populate(FeedContext context) {
            foreach (var feedItem in context.Response.Items.OfType<FeedItem<Comment>>()) {
                var comment = feedItem.Item;
                var commentedOn = _contentManager.Get(feedItem.Item.CommentedOn);
                var commentedOnInspector = new ItemInspector(
                    commentedOn,
                    _contentManager.GetItemMetadata(commentedOn));

                var title = T("Comment on {0} by {1}", commentedOnInspector.Title, comment.Author);

                //var inspector = new CommentInspector(
                //    feedItem.Item,
                //    _contentManager.GetItemMetadata(feedItem.Item));

                // add to known formats
                if (context.Format == "rss") {
                    var link = new XElement("link");
                    var guid = new XElement("guid", new XAttribute("isPermaLink", "false"));
                    context.Response.Contextualize(requestContext => {
                        var urlHelper = new UrlHelper(requestContext);
                        link.Add(urlHelper.RouteUrl(commentedOnInspector.Link) + "#comment-" + comment.Id);
                        guid.Add("urn:comment:" + comment.Id);
                    });

                    feedItem.Element.SetElementValue("title", title);
                    feedItem.Element.Add(link);
                    feedItem.Element.SetElementValue("description", comment.CommentText);
                    feedItem.Element.SetElementValue("pubDate", comment.CommentDate);//TODO: format
                    feedItem.Element.Add(guid);
                }
                else {
                    var feedItem1 = feedItem;
                    context.Response.Contextualize(requestContext => {
                        var urlHelper = new UrlHelper(requestContext);
                        context.FeedFormatter.AddProperty(context, feedItem1, "published-date", urlHelper.RouteUrl(commentedOnInspector.Link));
                    });
                    context.FeedFormatter.AddProperty(context, feedItem, "title", title.ToString());
                    context.FeedFormatter.AddProperty(context, feedItem, "description", comment.CommentText);

                    context.FeedFormatter.AddProperty(context, feedItem, "published-date", Convert.ToString(comment.CommentDate)); // format? cvt to generic T?
                }
            }
        }
    }
}
