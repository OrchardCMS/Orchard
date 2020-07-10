using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Feeds;

namespace Orchard.Blogs.Drivers {
    public class BlogPostPartDriver : ContentPartDriver<BlogPostPart> {
        private readonly IFeedManager _feedManager;
        private readonly IContentManager _contentManager;

        public BlogPostPartDriver(IFeedManager feedManager, IContentManager contentManager) {
            _feedManager = feedManager;
            _contentManager = contentManager;
        }

        protected override DriverResult Display(BlogPostPart part, string displayType, dynamic shapeHelper) {
            if (part.BlogPart != null && part.BlogPart.HasPublished()) {
                if (displayType.StartsWith("Detail")) {
                    var publishedBlog = part.BlogPart.IsPublished() ? part.BlogPart : _contentManager.Get(part.BlogPart.Id).As<BlogPart>();
                    var blogTitle = _contentManager.GetItemMetadata(publishedBlog).DisplayText;
                    _feedManager.Register(publishedBlog, blogTitle);
                }
            }
            return null;
        }
    }
}