using System.Linq;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;

namespace Orchard.Blogs.Handlers {
    [UsedImplicitly]
    public class BlogPostPartHandler : ContentHandler {
        private readonly IBlogService _blogService;
        private readonly IBlogPostService _blogPostService;

        public BlogPostPartHandler(IBlogService blogService, IBlogPostService blogPostService, RequestContext requestContext) {
            _blogService = blogService;
            _blogPostService = blogPostService;

            OnGetDisplayShape<BlogPostPart>(SetModelProperties);
            OnGetEditorShape<BlogPostPart>(SetModelProperties);
            OnUpdateEditorShape<BlogPostPart>(SetModelProperties);

            OnCreated<BlogPostPart>((context, part) => UpdateBlogPostCount(part));
            OnPublished<BlogPostPart>((context, part) => UpdateBlogPostCount(part));
            OnUnpublished<BlogPostPart>((context, part) => UpdateBlogPostCount(part));
            OnVersioned<BlogPostPart>((context, part, newVersionPart) => UpdateBlogPostCount(newVersionPart));
            OnRemoved<BlogPostPart>((context, part) => UpdateBlogPostCount(part));

            OnRemoved<BlogPart>(
                (context, b) =>
                blogPostService.Get(context.ContentItem.As<BlogPart>()).ToList().ForEach(
                    blogPost => context.ContentManager.Remove(blogPost.ContentItem)));
        }

        private void UpdateBlogPostCount(BlogPostPart blogPostPart) {
            CommonPart commonPart = blogPostPart.As<CommonPart>();
            if (commonPart != null &&
                commonPart.Record.Container != null) {

                BlogPart blogPart = blogPostPart.BlogPart ?? 
                    _blogService.Get(commonPart.Record.Container.Id, VersionOptions.Published).As<BlogPart>();

                // Ensure the "right" set of published posts for the blog is obtained
                blogPart.ContentItem.ContentManager.Flush();
                blogPart.PostCount = _blogPostService.PostCount(blogPart);
            }
        }

        private static void SetModelProperties(BuildShapeContext context, BlogPostPart blogPost) {
            context.Shape.Blog = blogPost.BlogPart;
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var blogPost = context.ContentItem.As<BlogPostPart>();
            
            if (blogPost == null)
                return;

            context.Metadata.CreateRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogPostAdmin"},
                {"Action", "Create"},
                {"blogId", blogPost.BlogPart.Id}
            };
            context.Metadata.EditorRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogPostAdmin"},
                {"Action", "Edit"},
                {"postId", context.ContentItem.Id},
                {"blogId", blogPost.BlogPart.Id}
            };
            context.Metadata.RemoveRouteValues = new RouteValueDictionary {
                {"Area", "Orchard.Blogs"},
                {"Controller", "BlogPostAdmin"},
                {"Action", "Delete"},
                {"postId", context.ContentItem.Id},
                {"blogId", blogPost.BlogPart.Id}
            };
        }
    }
}