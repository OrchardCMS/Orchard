using System;
using System.Linq;
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
        private readonly IRoutableService _routableService;
        private readonly IOrchardServices _orchardServices;

        public BlogPostHandler(IBlogPostService blogPostService, IRoutableService routableService, IOrchardServices orchardServices) {
            _blogPostService = blogPostService;
            _routableService = routableService;
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

            OnCreated<BlogPost>((context, bp) => updateBlogPostCount(bp.Blog));
            OnPublished<BlogPost>((context, bp) => updateBlogPostCount(bp.Blog));
            OnVersioned<BlogPost>((context, bp1, bp2) => updateBlogPostCount(bp2.Blog));
            OnRemoved<BlogPost>((context, bp) => updateBlogPostCount(bp.Blog));

            OnPublished<BlogPost>((context, bp) => ProcessSlug(bp));

            OnRemoved<Blog>(
                (context, b) =>
                blogPostService.Get(context.ContentItem.As<Blog>()).ToList().ForEach(
                    blogPost => context.ContentManager.Remove(blogPost.ContentItem)));
        }

        Localizer T { get; set; }

        private void ProcessSlug(BlogPost post) {
            _routableService.FillSlug(post.As<RoutableAspect>());

            var slugsLikeThis = _blogPostService.Get(post.Blog, VersionOptions.Published).Where(
                p => p.Slug.StartsWith(post.Slug, StringComparison.OrdinalIgnoreCase) &&
                     p.Id != post.Id).Select(p => p.Slug);

            //todo: (heskew) need better messages
            if (slugsLikeThis.Count() > 0) {
                //todo: (heskew) need better messages
                var originalSlug = post.Slug;
                post.Slug = _routableService.GenerateUniqueSlug(post.Slug, slugsLikeThis);

                if (originalSlug != post.Slug)
                    _orchardServices.Notifier.Warning(T("A different blog post is already published with this same slug ({0}) so a unique slug ({1}) was generated for this post.", originalSlug, post.Slug));
            }
        }
    }
}