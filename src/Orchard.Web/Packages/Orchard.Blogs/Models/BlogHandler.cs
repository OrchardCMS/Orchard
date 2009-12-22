using JetBrains.Annotations;
using Orchard.Blogs.Controllers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Blogs.Models {
    [UsedImplicitly]
    public class BlogHandler : ContentHandler {
        public BlogHandler(IRepository<BlogRecord> repository) {
            Filters.Add(new ActivatingFilter<Blog>(BlogDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<CommonAspect>(BlogDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<RoutableAspect>(BlogDriver.ContentType.Name));
            Filters.Add(new StorageFilter<BlogRecord>(repository));
        }
    }
}
