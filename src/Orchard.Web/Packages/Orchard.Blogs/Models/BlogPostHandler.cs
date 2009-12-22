using JetBrains.Annotations;
using Orchard.Blogs.Controllers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Blogs.Models {
    [UsedImplicitly]
    public class BlogPostHandler : ContentHandler {

        public BlogPostHandler(IRepository<BlogPostRecord> repository) {

            Filters.Add(new ActivatingFilter<BlogPost>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<CommonAspect>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<RoutableAspect>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<BodyAspect>(BlogPostDriver.ContentType.Name));
            Filters.Add(new StorageFilter<BlogPostRecord>(repository));


            OnCreated<BlogPost>((context, bp) => bp.Blog.PostCount++);
        }

    }
}