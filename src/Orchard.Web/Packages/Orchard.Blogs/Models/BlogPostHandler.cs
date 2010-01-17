using System.Linq;
using JetBrains.Annotations;
using Orchard.Blogs.Controllers;
using Orchard.Blogs.Services;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Records;
using Orchard.Core.Common.Services;
using Orchard.Data;

namespace Orchard.Blogs.Models {
    [UsedImplicitly]
    public class BlogPostHandler : ContentHandler {
        public BlogPostHandler(IRepository<CommonVersionRecord> commonRepository, IBlogPostService blogPostService, IRoutableService routableService) {
            Filters.Add(new ActivatingFilter<BlogPost>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<CommonAspect>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<RoutableAspect>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<BodyAspect>(BlogPostDriver.ContentType.Name));
            Filters.Add(new StorageFilter<CommonVersionRecord>(commonRepository));

            OnCreating<BlogPost>((context, blogPost) => {
                string slug = !string.IsNullOrEmpty(blogPost.Slug)
                                  ? blogPost.Slug
                                  : routableService.Slugify(blogPost.Title);

                blogPost.Slug = routableService.GenerateUniqueSlug(slug,
                                                       blogPostService.Get(blogPost.Blog, VersionOptions.Published).Where(
                                                           bp => bp.Slug.StartsWith(slug) && bp.Id != blogPost.Id).Select(
                                                           bp => bp.Slug));
            });

            OnCreated<BlogPost>((context, bp) => bp.Blog.PostCount++);
            OnRemoved<BlogPost>((context, bp) => bp.Blog.PostCount--);

            OnRemoved<Blog>(
                (context, bp) =>
                blogPostService.Get(context.ContentItem.As<Blog>()).ToList().ForEach(
                    blogPost => context.ContentManager.Remove(blogPost.ContentItem)));
        }
    }
}