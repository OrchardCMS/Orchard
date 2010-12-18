using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Extensions;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Feeds;
using Orchard.Localization;

namespace Orchard.Blogs.Drivers {
    [UsedImplicitly]
    public class BlogPostPartDriver : ContentPartDriver<BlogPostPart> {
        private readonly IFeedManager _feedManager;
        public IOrchardServices Services { get; set; }

        public BlogPostPartDriver(IOrchardServices services, IFeedManager feedManager) {
            _feedManager = feedManager;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return ""; } }

        protected override DriverResult Display(BlogPostPart part, string displayType, dynamic shapeHelper) {
            if (displayType.StartsWith("Detail"))
                _feedManager.Register(part.BlogPart);

            return null;
        }
    }
}