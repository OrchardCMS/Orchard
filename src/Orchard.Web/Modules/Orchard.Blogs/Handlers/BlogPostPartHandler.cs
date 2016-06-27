using System.Linq;
using System.Web.Routing;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;

namespace Orchard.Blogs.Handlers {
    public class BlogPostPartHandler : ContentHandler {
        private readonly IBlogService _blogService;

        public BlogPostPartHandler(IBlogService blogService, IBlogPostService blogPostService, RequestContext requestContext) {
            _blogService = blogService;

            OnGetDisplayShape<BlogPostPart>(SetModelProperties);
            OnGetEditorShape<BlogPostPart>(SetModelProperties);
            OnUpdateEditorShape<BlogPostPart>(SetModelProperties);

            OnCreated<BlogPostPart>((context, part) => ProcessBlogPostsCount(part));
            OnPublished<BlogPostPart>((context, part) => ProcessBlogPostsCount(part));
            OnUnpublished<BlogPostPart>((context, part) => ProcessBlogPostsCount(part));
            OnVersioned<BlogPostPart>((context, part, newVersionPart) => ProcessBlogPostsCount(newVersionPart));
            OnRemoved<BlogPostPart>((context, part) => ProcessBlogPostsCount(part));

            OnRemoved<BlogPart>(
                (context, b) =>
                blogPostService.Get(context.ContentItem.As<BlogPart>()).ToList().ForEach(
                    blogPost => context.ContentManager.Remove(blogPost.ContentItem)));
        }

        private void ProcessBlogPostsCount(BlogPostPart blogPostPart) {
            CommonPart commonPart = blogPostPart.As<CommonPart>();
            if (commonPart != null &&
                commonPart.Record.Container != null) {

                _blogService.ProcessBlogPostsCount(commonPart.Container.Id);
            }
        }

        private static void SetModelProperties(BuildShapeContext context, BlogPostPart blogPost) {
            context.Shape.Blog = blogPost.BlogPart;
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var blogPost = context.ContentItem.As<BlogPostPart>();

            // BlogPart can be null if this is a new Blog Post item.
            if (blogPost == null || blogPost.BlogPart == null) {
                return;
            }

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