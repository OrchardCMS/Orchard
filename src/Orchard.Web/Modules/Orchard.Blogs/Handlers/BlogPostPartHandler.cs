using System;
using System.Linq;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Orchard.Blogs.Handlers {
    [UsedImplicitly]
    public class BlogPostPartHandler : ContentHandler {
        private readonly IBlogPostService _blogPostService;
        private readonly IOrchardServices _orchardServices;

        public BlogPostPartHandler(IBlogService blogService, IBlogPostService blogPostService, IOrchardServices orchardServices, RequestContext requestContext) {
            _blogPostService = blogPostService;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;

            Action<BlogPart> updateBlogPostCount =
                (blog => {
                     // Ensure we get the "right" set of published posts for the blog
                     blog.ContentItem.ContentManager.Flush();

                     var postsCount = _blogPostService.Get(blog, VersionOptions.Published).Count();
                     blog.PostCount = postsCount;
                 });

            OnGetDisplayShape<BlogPostPart>(SetModelProperties);
            OnGetEditorShape<BlogPostPart>(SetModelProperties);
            OnUpdateEditorShape<BlogPostPart>(SetModelProperties);

            OnInitializing<BlogPostPart>((context, bp) => {
                var blogSlug = requestContext.RouteData.Values.ContainsKey("blogSlug") ? requestContext.RouteData.Values["blogSlug"] as string : null;
                if (!string.IsNullOrEmpty(blogSlug)) {
                    bp.BlogPart = blogService.Get(blogSlug);
                    return;
                }

                //todo: don't get at the container form data directly. right now the container is set in the common driver editor (updater)
                //todo: which is too late for what's needed (currently) in this handler
                var containerId = requestContext.HttpContext.Request.Form["CommonPart.containerId"];
                if (!string.IsNullOrEmpty(containerId)) {
                    int cId;
                    if (int.TryParse(containerId, out cId)) {
                        bp.BlogPart = context.ContentItem.ContentManager.Get(cId).As<BlogPart>();
                        return;
                    }
                }
            });
            OnCreated<BlogPostPart>((context, bp) => updateBlogPostCount(bp.BlogPart));
            OnPublished<BlogPostPart>((context, bp) => updateBlogPostCount(bp.BlogPart));
            OnVersioned<BlogPostPart>((context, bp1, bp2) => updateBlogPostCount(bp2.BlogPart));
            OnRemoved<BlogPostPart>((context, bp) => updateBlogPostCount(bp.BlogPart));

            OnRemoved<BlogPart>(
                (context, b) =>
                blogPostService.Get(context.ContentItem.As<BlogPart>()).ToList().ForEach(
                    blogPost => context.ContentManager.Remove(blogPost.ContentItem)));
        }

        private static void SetModelProperties(BuildShapeContext context, BlogPostPart blogPost) {
            context.Shape.Blog = blogPost.BlogPart;
        }

        Localizer T { get; set; }
    }
}