using JetBrains.Annotations;
using Orchard.Blogs.Drivers;
using Orchard.Blogs.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Routable.Models;
using Orchard.Data;

namespace Orchard.Blogs.Handlers {
    [UsedImplicitly]
    public class BlogHandler : ContentHandler {
        public BlogHandler(IRepository<BlogRecord> repository) {
            Filters.Add(new ActivatingFilter<Blog>(BlogDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<IsRoutable>(BlogDriver.ContentType.Name));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}