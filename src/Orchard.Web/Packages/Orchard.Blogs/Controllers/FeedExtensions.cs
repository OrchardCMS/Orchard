using System.Web.Routing;
using Orchard.Blogs.Models;
using Orchard.Core.Feeds;

namespace Orchard.Blogs.Controllers {
    public static class FeedExtensions {
        public static void Register(this IFeedManager feedManager, Blog blog) {
            feedManager.Register(blog.Name, "rss", new RouteValueDictionary { { "containerid", blog.Id } });
            feedManager.Register(blog.Name + " - Comments", "rss", new RouteValueDictionary { { "commentedoncontainer", blog.Id } });
        }
    }
}
