using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Extensions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Feeds;

namespace Orchard.Blogs.Drivers {
    [UsedImplicitly]
    public class BlogPostPartDriver : ContentPartDriver<BlogPostPart> {
        private readonly IFeedManager _feedManager;
        private readonly IContentManager _contentManager;

        public BlogPostPartDriver(IFeedManager feedManager, IContentManager contentManager) {
            _feedManager = feedManager;
            _contentManager = contentManager;
        }

        protected override DriverResult Display(BlogPostPart part, string displayType, dynamic shapeHelper) {
            if (displayType.StartsWith("Detail")) {
                var blogTitle = _contentManager.GetItemMetadata(part.BlogPart).DisplayText;
                _feedManager.Register(part.BlogPart, blogTitle);
            }

            return null;
        }
    }
}