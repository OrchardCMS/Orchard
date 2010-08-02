using System.Web.Routing;
using Orchard.Blogs.Models;
using Orchard.Core.Feeds;

namespace Orchard.Blogs.Extensions {
    public static class FeedManagerExtensions {
        public static void Register(this IFeedManager feedManager, BlogPart blogPart) {
            feedManager.Register(blogPart.Name, "rss", new RouteValueDictionary { { "containerid", blogPart.Id } });
            feedManager.Register(blogPart.Name + " - Comments", "rss", new RouteValueDictionary { { "commentedoncontainer", blogPart.Id } });
        }
    }
}
