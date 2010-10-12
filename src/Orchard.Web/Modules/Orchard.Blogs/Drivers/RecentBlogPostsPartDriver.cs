using System.Collections.Generic;
using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.ContentsLocation.Models;

namespace Orchard.Blogs.Drivers {
    public class RecentBlogPostsPartDriver : ContentPartDriver<RecentBlogPostsPart> {
        private readonly IBlogService _blogService;
        private readonly IContentManager _contentManager;

        public RecentBlogPostsPartDriver(IBlogService blogService, IContentManager contentManager) {
            _blogService = blogService;
            _contentManager = contentManager;
        }

        protected override DriverResult Display(RecentBlogPostsPart part, string displayType, dynamic shapeHelper) {
            IEnumerable<BlogPostPart> blogPosts = null;

            BlogPart blog = null;
            if (!string.IsNullOrWhiteSpace(part.ForBlog))
                blog = _blogService.Get(part.ForBlog);

            if (blog != null) {
                blogPosts = _contentManager.Query(VersionOptions.Published, "BlogPost")
                    .Join<CommonPartRecord>().Where(cr => cr.Container == blog.Record.ContentItemRecord)
                    .OrderByDescending(cr => cr.CreatedUtc)
                    .Slice(0, part.Count)
                    .Select(ci => ci.As<BlogPostPart>());
            }
            else {
                var blogs = _blogService.Get().ToList();
                blogPosts = _contentManager.Query(VersionOptions.Published, "BlogPost")
                    .Join<CommonPartRecord>()
                    .OrderByDescending(cr => cr.CreatedUtc)
                    .Slice(0, part.Count)
                    .Select(ci => ci.As<BlogPostPart>());
            }

            var list = shapeHelper.List();
            list.AddRange(blogPosts.Select(bp => _contentManager.BuildDisplay(bp, "Summary.BlogPost")));

            var blogPostList = shapeHelper.Parts_Blogs_BlogPost_List(ContentPart: part, BlogPosts: list);
            blogPostList.Metadata.Type = "Parts_Blogs_BlogPost.List";

            return ContentShape(blogPostList).Location("Primary");
        }

        protected override DriverResult Editor(RecentBlogPostsPart part, dynamic shapeHelper) {
            var location = part.GetLocation("Editor", "Primary", "5");
            return ContentPartTemplate(part, "Parts/Blogs.RecentBlogPosts").Location(location);
        }

        protected override DriverResult Editor(RecentBlogPostsPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}