using JetBrains.Annotations;
using Orchard.Blogs.Controllers;
using Orchard.Core.Common.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Records;
using Orchard.Data;

namespace Orchard.Blogs.Models {
    [UsedImplicitly]
    public class BlogPostHandler : ContentHandler {
        public BlogPostHandler(IRepository<CommonVersionRecord> commonRepository) {
            Filters.Add(new ActivatingFilter<BlogPost>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<CommonAspect>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<RoutableAspect>(BlogPostDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<BodyAspect>(BlogPostDriver.ContentType.Name));
            Filters.Add(new StorageFilter<CommonVersionRecord>(commonRepository));

            OnCreated<BlogPost>((context, bp) => bp.Blog.PostCount++);
        }
    }
}