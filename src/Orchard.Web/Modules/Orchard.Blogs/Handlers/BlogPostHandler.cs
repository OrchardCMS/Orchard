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
using Orchard.Core.Common.Services;
using Orchard.Localization;
using Orchard.UI.Notify;

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
            Filters.Add(new ActivatingFilter<ContentPart<CommonVersionRecord>>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<RoutableAspect>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<BodyAspect>(BlogPostDriver.ContentType.Name));

            OnLoaded<BlogPost>((context, p) => p.ScheduledPublishUtc = _blogPostService.GetScheduledPublishUtc(p));

            Action<Blog> updateBlogPostCount =
                (blog => {
                     // Ensure we get the "right" set of published posts for the blog
                     blog.ContentItem.ContentManager.Flush();

                     var posts = _blogPostService.Get(blog, VersionOptions.Published).ToList();
                     blog.PostCount = posts.Count;
                 });

            OnActivated<BlogPost>((context, bp) => {
                var blogSlug = requestContext.RouteData.Values.ContainsKey("blogSlug") ? requestContext.RouteData.Values["blogSlug"] as string : null;
                if (!string.IsNullOrEmpty(blogSlug)) {
                    bp.Blog = blogService.Get(blogSlug);
                    return;
                }

                var containerId = requestContext.HttpContext.Request.Form["containerId"];
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