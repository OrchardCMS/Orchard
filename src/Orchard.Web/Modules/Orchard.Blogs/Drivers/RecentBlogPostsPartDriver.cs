using System.Collections.Generic;
using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;

namespace Orchard.Blogs.Drivers {
    public class RecentBlogPostsPartDriver : ContentPartDriver<RecentBlogPostsPart> {
        private readonly IBlogService _blogService;
        private readonly IContentManager _contentManager;

        public RecentBlogPostsPartDriver(IBlogService blogService, IContentManager contentManager) {
            _blogService = blogService;
            _contentManager = contentManager;
        }

        protected override DriverResult Display(RecentBlogPostsPart part, string displayType, dynamic shapeHelper) {
            IEnumerable<BlogPostPart> blogPosts;

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
                blogPosts = _contentManager.Query(VersionOptions.Published, "BlogPost")
                    .Join<CommonPartRecord>()
                    .OrderByDescending(cr => cr.CreatedUtc)
                    .Slice(0, part.Count)
                    .Select(ci => ci.As<BlogPostPart>());
            }

            var list = shapeHelper.List();
            list.AddRange(blogPosts.Select(bp => _contentManager.BuildDisplay(bp, "Summary")));

            var blogPostList = shapeHelper.Parts_Blogs_BlogPost_List(ContentPart: part, ContentItems: list);

            return ContentShape(shapeHelper.Parts_Blogs_RecentBlogPosts(ContentItem: part.ContentItem, ContentItems: blogPostList));
        }

        protected override DriverResult Editor(RecentBlogPostsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Blogs_RecentBlogPosts_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Blogs.RecentBlogPosts", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(RecentBlogPostsPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}