using System.Linq;
using JetBrains.Annotations;
using Orchard.Blogs.Controllers;
using Orchard.Blogs.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Core.Common.Services;
using Orchard.Data;

namespace Orchard.Blogs.Models {
    [UsedImplicitly]
    public class BlogHandler : ContentHandler {
        public BlogHandler(IRepository<BlogRecord> repository, IBlogService blogService,
                           IRoutableService routableService) {
            Filters.Add(new ActivatingFilter<Blog>(BlogDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<CommonAspect>(BlogDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<RoutableAspect>(BlogDriver.ContentType.Name));
            Filters.Add(new StorageFilter<BlogRecord>(repository));

            OnCreating<Blog>((context, blog) => {
                string slug = !string.IsNullOrEmpty(blog.Slug)
                                  ? blog.Slug
                                  : routableService.Slugify(blog.Name);

                blog.Slug = routableService.GenerateUniqueSlug(slug,
                                                       blogService.Get().Where(
                                                           b => b.Slug.StartsWith(slug) && b.Id != blog.Id).Select(
                                                           b => b.Slug));
            });
        }
    }
}