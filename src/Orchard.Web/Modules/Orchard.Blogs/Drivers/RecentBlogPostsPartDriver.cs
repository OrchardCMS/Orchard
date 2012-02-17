using System;
using System.Linq;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.Blogs.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;

namespace Orchard.Blogs.Drivers {
    public class RecentBlogPostsPartDriver : ContentPartDriver<RecentBlogPostsPart> {
        private readonly IBlogService _blogService;
        private readonly IContentManager _contentManager;

        public RecentBlogPostsPartDriver(
            IBlogService blogService, 
            IContentManager contentManager) {
            _blogService = blogService;
            _contentManager = contentManager;
        }

        protected override DriverResult Display(RecentBlogPostsPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_Blogs_RecentBlogPosts", () => {
            var blog = _contentManager.Get<BlogPart>(part.BlogId);

                if (blog == null) {
                    return null;
                }

                var blogPosts = _contentManager.Query(VersionOptions.Published, "BlogPost")
                    .Join<CommonPartRecord>().Where(cr => cr.Container == blog.Record.ContentItemRecord)
                    .OrderByDescending(cr => cr.CreatedUtc)
                    .Slice(0, part.Count)
                    .Select(ci => ci.As<BlogPostPart>());

                var list = shapeHelper.List();
                list.AddRange(blogPosts.Select(bp => _contentManager.BuildDisplay(bp, "Summary")));

                var blogPostList = shapeHelper.Parts_Blogs_BlogPost_List(ContentItems: list);

                return shapeHelper.Parts_Blogs_RecentBlogPosts(ContentItems: blogPostList, Blog: blog);
            });
        }

        protected override DriverResult Editor(RecentBlogPostsPart part, dynamic shapeHelper) {
            var viewModel = new RecentBlogPostsViewModel {
                Count = part.Count,
                BlogId = part.BlogId,
                Blogs = _blogService.Get().ToList().OrderBy(b => b.Name)
            };

            return ContentShape("Parts_Blogs_RecentBlogPosts_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts.Blogs.RecentBlogPosts", Model: viewModel, Prefix: Prefix));
        }

        protected override DriverResult Editor(RecentBlogPostsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new RecentBlogPostsViewModel();

            if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                part.BlogId = viewModel.BlogId;
                part.Count = viewModel.Count;
            }

            return Editor(part, shapeHelper);
        }

        protected override void Importing(RecentBlogPostsPart part, ImportContentContext context) {
            var blog = context.Attribute(part.PartDefinition.Name, "Blog");
            if (blog != null) {
                part.BlogId = context.GetItemFromSession(blog).Id;
            }

            var count = context.Attribute(part.PartDefinition.Name, "Count");
            if (count != null) {
                part.Count = Convert.ToInt32(count);
            }
        }

        protected override void Exporting(RecentBlogPostsPart part, ExportContentContext context) {
            var blog = _contentManager.Get(part.BlogId);
            var blogIdentity = _contentManager.GetItemMetadata(blog).Identity;
            context.Element(part.PartDefinition.Name).SetAttributeValue("Blog", blogIdentity);

            context.Element(part.PartDefinition.Name).SetAttributeValue("Count", part.Count);
        }
    }
}