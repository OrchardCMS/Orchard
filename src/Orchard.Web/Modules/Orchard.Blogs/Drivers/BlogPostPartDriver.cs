using Orchard.Blogs.Models;
using Orchard.Blogs.Extensions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Feeds;

namespace Orchard.Blogs.Drivers {
    public class BlogPostPartDriver : ContentPartDriver<BlogPostPart> {
        private readonly IFeedManager _feedManager;
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContextAccessor;

        public BlogPostPartDriver(IFeedManager feedManager, IContentManager contentManager, IWorkContextAccessor workContextAccessor) {
            _feedManager = feedManager;
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
        }

        protected override DriverResult Display(BlogPostPart part, string displayType, dynamic shapeHelper) {
            if (part.BlogPart != null && part.BlogPart.HasPublished()) {
                if (displayType.StartsWith("Detail")) {
                    var blogTitle = _contentManager.GetItemMetadata(part.BlogPart).DisplayText;
                    _feedManager.Register(part.BlogPart, blogTitle);
                }
            }
            else {
                if (displayType.StartsWith("Detail") || displayType == "Summary") {
                    _workContextAccessor.GetContext().HttpContext.Response.RedirectToRoute(new { Area = "Common", Controller = "Error", Action = "NotFound" });
                }
            }
            return null;
        }
    }
}