using System;
using System.Web.Routing;
using Orchard.Blogs.Models;
using Orchard.Core.Feeds;

namespace Orchard.Blogs.Extensions {
    public static class FeedManagerExtensions {
        public static void Register(this IFeedManager feedManager, BlogPart blogPart, string blogTitle) {

            if (String.IsNullOrWhiteSpace(blogPart.FeedProxyUrl)) {
                feedManager.Register(blogTitle, "rss", new RouteValueDictionary {{"containerid", blogPart.Id}});
            }
            else {
                feedManager.Register(blogTitle, "rss", blogPart.FeedProxyUrl);
            }

            if (blogPart.EnableCommentsFeed) {
                feedManager.Register(blogTitle + " - Comments", "rss", new RouteValueDictionary {{"commentedoncontainer", blogPart.Id}});
            }
        }
    }
}
