using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Blogs.Extensions;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.ContentsLocation.Models;
using Orchard.Core.Feeds;
using Orchard.DisplayManagement;
using Orchard.Localization;

namespace Orchard.Blogs.Drivers {
    [UsedImplicitly]
    public class BlogPartDriver : ContentPartDriver<BlogPart> {
        public IOrchardServices Services { get; set; }

        private readonly IContentManager _contentManager;
        private readonly IBlogPostService _blogPostService;
        private readonly IFeedManager _feedManager;

        public BlogPartDriver(IOrchardServices services, IContentManager contentManager, IBlogPostService blogPostService, IFeedManager feedManager, IShapeHelperFactory shapeHelperFactory) {
            Services = services;
            _contentManager = contentManager;
            _blogPostService = blogPostService;
            _feedManager = feedManager;
            T = NullLocalizer.Instance;
            Shape = shapeHelperFactory.CreateHelper();
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix { get { return ""; } }

        protected override DriverResult Display(BlogPart blogPart, string displayType) {

            IEnumerable<dynamic > blogPosts = null;
            if (displayType.StartsWith("Admin")) {
                blogPosts = _blogPostService.Get(blogPart, VersionOptions.Latest)
                    .Select(bp => _contentManager.BuildDisplayModel(bp, "SummaryAdmin.BlogPost"));
            }
            else if (!displayType.Contains("Summary")) {
                blogPosts = _blogPostService.Get(blogPart)
                    .Select(bp => _contentManager.BuildDisplayModel(bp, "Summary.BlogPost"));
                _feedManager.Register(blogPart);
            }

            var blogPostList = Shape.List();
            blogPostList.AddRange(blogPosts);

            return Combined(
                ContentPartTemplate(blogPart, "Parts/Blogs.Blog.Manage").Location("manage"),
                ContentPartTemplate(blogPart, "Parts/Blogs.Blog.Metadata").Location("metadata"),
                ContentPartTemplate(blogPart, "Parts/Blogs.Blog.Description").Location("manage", "after"),
                ContentPartTemplate(blogPostList, "Parts/Blogs.BlogPost.List").LongestMatch(displayType, "Admin").Location("primary"));
        }

        protected override DriverResult Editor(BlogPart blogPart) {
            var location = blogPart.GetLocation("Editor");
            return Combined(
                ContentPartTemplate(blogPart, "Parts/Blogs.Blog.Fields").Location(location));
        }

        protected override DriverResult Editor(BlogPart blogPart, IUpdateModel updater) {
            updater.TryUpdateModel(blogPart, Prefix, null, null);
            return Editor(blogPart);
        }
    }
}