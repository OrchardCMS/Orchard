using System;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Blogs.Controllers;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Records;
using Orchard.Data;

namespace Orchard.Blogs.Models {
    [UsedImplicitly]
    public class BlogPostHandler : ContentHandler {
        public BlogPostHandler(IRepository<CommonVersionRecord> commonRepository, IBlogPostService blogPostService) {
            Filters.Add(new ActivatingFilter<BlogPost>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<CommonAspect>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<RoutableAspect>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<BodyAspect>(BlogPostDriver.ContentType.Name));
            Filters.Add(new StorageFilter<CommonVersionRecord>(commonRepository));

            Action<Blog> updateBlogPostCount =
                (blog => {
                    // Ensure we get the "right" set of published posts for the blog
                    blog.ContentItem.ContentManager.Flush();

                    var posts = blogPostService.Get(blog, VersionOptions.Published).ToList();
                    blog.PostCount = posts.Count;
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
    }
}