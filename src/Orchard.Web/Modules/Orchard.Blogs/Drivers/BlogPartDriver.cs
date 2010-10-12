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

        protected override DriverResult Display(BlogPart part, string displayType, dynamic shapeHelper) {
            var driverResults = new List<DriverResult>();

            var metadata = shapeHelper.Parts_Blogs_Blog_Manage(ContentPart: part);
            metadata.Metadata.Type = "Parts_Blogs_Blog.Manage";
            driverResults.Add(ContentShape(metadata).Location("manage"));

            var description = shapeHelper.Parts_Blogs_Blog_Description(ContentPart: part);
            description.Metadata.Type = "Parts_Blogs_Blog.Description";
            driverResults.Add(ContentShape(description).Location("manage", "after"));

            if (displayType.StartsWith("Admin")) {
                var list = shapeHelper.List();
                list.AddRange(_blogPostService.Get(part, VersionOptions.Latest)
                                          .Select(bp => _contentManager.BuildDisplay(bp, "SummaryAdmin.BlogPost")));
                var blogPostList = shapeHelper.Parts_Blogs_BlogPost_List(ContentPart: part, BlogPosts: list);
                blogPostList.Metadata.Type = "Parts_Blogs_BlogPost.List.Admin";
                var contentShape = ContentShape(blogPostList).Location("Primary");
                driverResults.Add(contentShape);
            }
            else if (!displayType.Contains("Summary")) {
                var list = shapeHelper.List();
                list.AddRange(_blogPostService.Get(part)
                                          .Select(bp => _contentManager.BuildDisplay(bp, "Summary.BlogPost")));
                var blogPostList = shapeHelper.Parts_Blogs_BlogPost_List(ContentPart: part, BlogPosts: list);
                blogPostList.Metadata.Type = "Parts_Blogs_BlogPost.List";
                var contentShape = ContentShape(blogPostList).Location("Primary");
                driverResults.Add(contentShape);

                _feedManager.Register(part);
            }

            return Combined(driverResults.ToArray());
        }

        protected override DriverResult Editor(BlogPart blogPart, dynamic shapeHelper) {
            var location = blogPart.GetLocation("Editor");
            return Combined(
                ContentPartTemplate(blogPart, "Parts/Blogs.Blog.Fields").Location(location));
        }

        protected override DriverResult Editor(BlogPart blogPart, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(blogPart, Prefix, null, null);
            return Editor(blogPart, shapeHelper);
        }
    }
}