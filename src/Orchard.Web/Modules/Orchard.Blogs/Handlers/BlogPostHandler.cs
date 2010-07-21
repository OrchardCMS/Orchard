using System;
using System.Linq;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Blogs.Drivers;
using Orchard.Blogs.Models;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.PublishLater.Models;
using Orchard.Core.Routable.Models;
using Orchard.Localization;

namespace Orchard.Blogs.Handlers {
    [UsedImplicitly]
    public class BlogPostHandler : ContentHandler {
        private readonly IBlogPostService _blogPostService;
        private readonly IOrchardServices _orchardServices;

        public BlogPostHandler(IBlogService blogService, IBlogPostService blogPostService, IOrchardServices orchardServices, RequestContext requestContext) {
            _blogPostService = blogPostService;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;

            Filters.Add(new ActivatingFilter<BlogPost>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<CommonAspect>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<PublishLaterPart>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<ContentPart<CommonVersionRecord>>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<IsRoutable>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<BodyAspect>(BlogPostDriver.ContentType.Name));

            Action<Blog> updateBlogPostCount =
                (blog => {
                     // Ensure we get the "right" set of published posts for the blog
                     blog.ContentItem.ContentManager.Flush();

                     var postsCount = _blogPostService.Get(blog, VersionOptions.Published).Count();
                     blog.PostCount = postsCount;
                 });

            OnInitializing<BlogPost>((context, bp) => {
                var blogSlug = requestContext.RouteData.Values.ContainsKey("blogSlug") ? requestContext.RouteData.Values["blogSlug"] as string : null;
                if (!string.IsNullOrEmpty(blogSlug)) {
                    bp.Blog = blogService.Get(blogSlug);
                    return;
                }

                //todo: don't get at the container form data directly. right now the container is set in the common driver editor (updater)
                //todo: which is too late for what's needed (currently) in this handler
                var containerId = requestContext.HttpContext.Request.Form["CommonAspect.containerId"];
                if (!string.IsNullOrEmpty(containerId)) {
                    int cId;
                    if (int.TryParse(containerId, out cId)) {
                        bp.Blog = context.ContentItem.ContentManager.Get(cId).As<Blog>();
                        return;
                    }
                }
            });
            OnCreated<BlogPost>((context, bp) => updateBlogPostCount(bp.Blog));
            OnPublished<BlogPost>((context, bp) => updateBlogPostCount(bp.Blog));
            OnVersioned<BlogPost>((context, bp1, bp2) => updateBlogPostCount(bp2.Blog));
            OnRemoved<BlogPost>((context, bp) => updateBlogPostCount(bp.Blog));

            OnRemoved<Blog>(
                (context, b) =>
                blogPostService.Get(context.ContentItem.As<Blog>()).ToList().ForEach(
                    blogPost => context.ContentManager.Remove(blogPost.ContentItem)));
        }

        Localizer T { get; set; }
    }
}