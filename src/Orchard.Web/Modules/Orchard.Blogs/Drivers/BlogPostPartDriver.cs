using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Extensions;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Feeds;

namespace Orchard.Blogs.Drivers {
    [UsedImplicitly]
    public class BlogPostPartDriver : ContentPartDriver<BlogPostPart> {
        private readonly IFeedManager _feedManager;

        public BlogPostPartDriver(IFeedManager feedManager) {
            _feedManager = feedManager;
        }

        protected override DriverResult Display(BlogPostPart part, string displayType, dynamic shapeHelper) {
            if (displayType.StartsWith("Detail"))
                _feedManager.Register(part.BlogPart);

            return null;
        }
    }
}