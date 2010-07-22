using JetBrains.Annotations;
using Orchard.Blogs.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Orchard.Blogs.Handlers {
    [UsedImplicitly]
    public class BlogHandler : ContentHandler {
        public BlogHandler(IRepository<BlogRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}