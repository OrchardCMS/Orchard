using System.Linq;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Blogs.Handlers {
    [UsedImplicitly]
    public class BlogPostPartHandler : ContentHandler {

        public BlogPostPartHandler(IBlogService blogService, IBlogPostService blogPostService, RequestContext requestContext) {

            OnGetDisplayShape<BlogPostPart>(SetModelProperties);
            OnGetEditorShape<BlogPostPart>(SetModelProperties);
            OnUpdateEditorShape<BlogPostPart>(SetModelProperties);

            OnCreated<BlogPostPart>((context, part) => blogService.ProcessBlogPostsCount(part.BlogPart.Id));
            OnPublished<BlogPostPart>((context, part) => blogService.ProcessBlogPostsCount(part.BlogPart.Id));
            OnUnpublished<BlogPostPart>((context, part) => blogService.ProcessBlogPostsCount(part.BlogPart.Id));
            OnVersioned<BlogPostPart>((context, part, newVersionPart) => blogService.ProcessBlogPostsCount(newVersionPart.BlogPart.Id));
            OnRemoved<BlogPostPart>((context, part) => blogService.ProcessBlogPostsCount(part.BlogPart.Id));

            OnRemoved<BlogPart>(
                (context, b) =>
                blogPostService.Get(context.ContentItem.As<BlogPart>()).ToList().ForEach(
                    blogPost => context.ContentManager.Remove(blogPost.ContentItem)));
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